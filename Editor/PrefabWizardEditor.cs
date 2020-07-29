using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityPrefabWizard.AssetUtilities;

namespace UnityPrefabWizard.Editor
{
    public class PrefabWizardEditor : EditorWindow
    {
        private readonly Dictionary<string, string> _defaultShaderPropertyToTextureSuffixMatching = 
            new Dictionary<string, string>()
        {
            {"_MainTex", "_Diff"}, // Albedo or diffuse texture
            {"_SpecGlossMap", "_Spec"}, // Specular texture
            {"_BumpMap", "_Norm"}, // Normal map texture
            // ...
        };

        private VisualElement _root;
        private VisualTreeAsset _contents;

        private ListView _rulesListView;
        private VisualTreeAsset _singleRuleVisualTree;
        private VisualElement _singleRuleVisualElement;

        private int _ruleCount;
        private List<int> _availableIds;

        private Button _loadRules;
        private Button _saveRules;
        private Button _addRule;
        private Button _clearRules;
        private Button _createPrefab;

        private ListView _logListView;
        private VisualTreeAsset _listEntryVisualTreeAsset;
        private Button _listEntryButton;

        private Button _clearLog;
        private Label _labelVersion;

        private List<Rule> _activeRuleList;

        [MenuItem(Constants.WindowMenuPath)]                                                                                     
        public static void ShowWindow()                                                                                                      
        {
            var window = GetWindow<PrefabWizardEditor>();
            window.titleContent = new GUIContent(Constants.WindowName);
            window.minSize = new Vector2(Constants.WindowWidth, Constants.WindowHeight);
        }                                                                                                                                    
                                                                                                                                             
        private void OnEnable()
        {
            _ruleCount = -1;
            _activeRuleList = new List<Rule>();
            _availableIds = new List<int>();
            
            // Reference to the root of the window.                                                                                          
            _root = rootVisualElement;
            
            // Instantiate contents
            _contents = Resources.Load<VisualTreeAsset>(Constants.LabelUxmlMainPrefabWizard);
            _contents.CloneTree(_root);

            _rulesListView = _root.Q<ListView>(Constants.LabelListViewRulesList);
            
            // Single Rule Reference
            _singleRuleVisualTree = Resources.Load<VisualTreeAsset>(Constants.LabelUxmlSingleRule);

            // Load Rules Button
            _loadRules = _root.Q<Button>(Constants.LabelButtonLoadRules);
            _loadRules.clickable.clicked += LoadRules;

            // Save Rules Button
            _saveRules = _root.Q<Button>(Constants.LabelButtonSaveRules);
            _saveRules.clickable.clicked += SaveRules;
            
            // Add Rule Button
            _addRule = _root.Q<Button>(Constants.LabelButtonAddRule);
            _addRule.clickable.clicked += AddNewRule;
            
            // Clear Rules Button
            _clearRules = _root.Q<Button>(Constants.LabelButtonClearRules);
            _clearRules.clickable.clicked += ClearRules;
            
            // Create Prefab Button
            _createPrefab = _root.Q<Button>(Constants.LabelButtonCreatePrefab);
            _createPrefab.clickable.clicked += CreatePrefabForSelectedMesh;
            
            // Log List View
            _logListView = _root.Q<ListView>(Constants.LabelListViewLog);
            _listEntryVisualTreeAsset = Resources.Load<VisualTreeAsset>(Constants.LabelUxmlLogEntry);
            _logListView.Clear();
            
            // ClearLog
            _clearLog = _root.Q<Button>(Constants.LabelButtonClearLog);
            _clearLog.clickable.clicked += ClearLog;
            
            // Version Label
            _labelVersion = _root.Q<Label>(Constants.LabelLabelVersion);
            _labelVersion.text = Constants.VersionNumber;
        }

        private void LoadRules()
        {
            var rulesPath = EditorUtility.OpenFilePanel("", "", Constants.JsonExtension);
            if (String.IsNullOrWhiteSpace(rulesPath))
            {
                
                return;
            }
            
            _activeRuleList = PrefabWizard.GetRules(rulesPath);
            UpdateRulesListViewContentsWithActiveRuleList();
            
            // Update the log list
            _logListView.Clear(); 
            _listEntryVisualTreeAsset.CloneTree(_logListView);
            _listEntryButton = _root.Q<Button>(Constants.LabelButtonLogEntry);
            _listEntryButton.text = Constants.MessageSuccessfullyLoadedRules + rulesPath;
            _logListView.Add(_listEntryButton);
        }

        private void SaveRules()
        {
            var rulesPath = EditorUtility.SaveFilePanel(
                "", "", Constants.DefaultRulesFileName, Constants.JsonExtension);

            if (String.IsNullOrWhiteSpace(rulesPath))
            {
                return;
            }

            UpdateActiveRuleListWithRulesListViewContents();
            
            PrefabWizard.SetRules(_activeRuleList, rulesPath);
            AssetDatabase.Refresh();
            
            // Update the log list
            _logListView.Clear(); 
            _listEntryVisualTreeAsset.CloneTree(_logListView);
            _listEntryButton = _root.Q<Button>(Constants.LabelButtonLogEntry);
            _listEntryButton.text = Constants.MessageSuccessfullySavedRules + rulesPath;
            _logListView.Add(_listEntryButton);
        }

        private void AddNewRule()
        {
            var newRule = GetNewRule();
        }

        private VisualElement GetNewRule()
        {
            var id = GetNextAvailableId();
            
            _singleRuleVisualTree.CloneTree(
                _rulesListView.hierarchy.ElementAt(0
                ).hierarchy.ElementAt(0
                ).hierarchy.ElementAt(0));
            
            // Get a random color for the button and determine if the X text should be black or white
            var randomColor = GeneralUtilities.GetRandomColor();
            var inverseRandomColor = 
                (randomColor.r + randomColor.g + randomColor.b) / 3 > 0.5f ? Color.black : Color.white;
            
            var newRuleVisualElement = _root.Q<VisualElement>(Constants.LabelVisualElementSingleRule);
            newRuleVisualElement.name += id.ToString();
            
            newRuleVisualElement.style.borderTopColor = new StyleColor(randomColor);
            newRuleVisualElement.style.borderRightColor = new StyleColor(randomColor);
            newRuleVisualElement.style.borderBottomColor = new StyleColor(randomColor);
            newRuleVisualElement.style.borderLeftColor = new StyleColor(randomColor);
            
            // 'Name starts with' foldout
            var nameStartsWithFoldout = newRuleVisualElement.Q<Foldout>(Constants.LabelFoldoutIncludeNameStartsWith);

            // Add new field Button for 'name starts with'
            var nameStartsWithButton = newRuleVisualElement.Q<Button>(Constants.LabelButtonNameStartsWith);
            nameStartsWithButton.clickable.clicked += () => AddNewSingleEntryToFoldout(nameStartsWithFoldout);
            
            // 'Name contains' foldout
            var nameContainsFoldout = newRuleVisualElement.Q<Foldout>(Constants.LabelFoldoutIncludeNameContains);

            // Add new field Button for 'name contains'
            var nameContainsButton = newRuleVisualElement.Q<Button>(Constants.LabelButtonNameContains);
            nameContainsButton.clickable.clicked += () => AddNewSingleEntryToFoldout(nameContainsFoldout);
            
            // Shader slot
            var shaderObjectField = newRuleVisualElement.Q<ObjectField>(Constants.LabelObjectFieldShader);
            shaderObjectField.objectType = typeof(Shader);
            
            // 'Texture Input' foldout
            var textureInputFoldout = newRuleVisualElement.Q<Foldout>(Constants.LabelFoldoutTextureInputs);
            
            // Default Values for 'Texture Input' foldout
            foreach (var mapping in _defaultShaderPropertyToTextureSuffixMatching)
            {
                AddNewDoubleEntryToFoldout(
                    textureInputFoldout,
                    mapping.Key,
                    mapping.Value);
            }

            // Add new field Button for 'texture inputs'
            var textureInputsButton = newRuleVisualElement.Q<Button>(Constants.LabelButtonAddTextureInputMatching);
            textureInputsButton.clickable.clicked += () => AddNewDoubleEntryToFoldout(textureInputFoldout);

            // Remove Button
            var removeButton = newRuleVisualElement.Q<Button>(Constants.LabelButtonRemove);
            removeButton.clickable.clicked += () => RemoveRule(newRuleVisualElement, id);
            removeButton.style.backgroundColor = randomColor;
            removeButton.style.color = inverseRandomColor;

            return newRuleVisualElement;
        }

        private void AddNewSingleEntryToFoldout(Foldout foldout, string entryText="")
        {
            var newIncludeNameStartsWithVisualElement = new VisualElement();
            newIncludeNameStartsWithVisualElement.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

            var newIncludeNameStartsWithTextField = new TextField {value = entryText};
            newIncludeNameStartsWithTextField.style.flexGrow = 1;
            
            var newIncludeNameStartsWithButton = new Button()
            {
                text = "X"
            };
            newIncludeNameStartsWithButton.style.borderTopLeftRadius = 0;
            newIncludeNameStartsWithButton.style.borderTopRightRadius = 0;
            newIncludeNameStartsWithButton.style.borderBottomRightRadius = 0;
            newIncludeNameStartsWithButton.style.borderBottomLeftRadius = 0;
            newIncludeNameStartsWithButton.clickable.clicked += () => 
                RemoveVisualElementFromFoldout(
                    newIncludeNameStartsWithVisualElement,
                    foldout);
            
            newIncludeNameStartsWithVisualElement.Add(newIncludeNameStartsWithButton);
            newIncludeNameStartsWithVisualElement.Add(newIncludeNameStartsWithTextField);
            
            foldout.Add(newIncludeNameStartsWithVisualElement);
        }
        
        private void AddNewDoubleEntryToFoldout(Foldout foldout, string textFirstInput="", string textSecondInput="")
        {
            var newTextureInputField = new VisualElement();
            newTextureInputField.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            
            var removeButton = new Button()
            {
                text = "X"
            };
            removeButton.style.borderTopLeftRadius = 0;
            removeButton.style.borderTopRightRadius = 0;
            removeButton.style.borderBottomRightRadius = 0;
            removeButton.style.borderBottomLeftRadius = 0;
            removeButton.clickable.clicked += () => 
                RemoveVisualElementFromFoldout(
                    newTextureInputField,
                    foldout);
            
            var shaderTextField = new TextField();
            shaderTextField.style.flexGrow = 1;
            shaderTextField.value = textFirstInput;

            var label = new Label {text = "to"};

            var textureSuffixTextField = new TextField();
            textureSuffixTextField.style.flexGrow = 1;
            textureSuffixTextField.value = textSecondInput;

            newTextureInputField.Add(removeButton);
            newTextureInputField.Add(shaderTextField);
            newTextureInputField.Add(label);
            newTextureInputField.Add(textureSuffixTextField);
            
            foldout.Add(newTextureInputField);
        }
        
        private void RemoveVisualElementFromFoldout(VisualElement visualElement, Foldout foldout)
        {
            foldout.Remove(visualElement);
        }

        private void RemoveRule(VisualElement singleRule, int id)
        {
            _rulesListView.Remove(singleRule);
            _availableIds.Add(id);
        }

        private void ClearRules()
        {
            _rulesListView.Clear();
            _availableIds.Clear();
            _ruleCount = -1;
        }

        private int GetNextAvailableId()
        {
            // If there are no available IDs, increment the max ...
            if (_availableIds.Count == 0)
            {
                _ruleCount++;
                return _ruleCount;
            }

            // If there are available IDs, return the first one and eliminate it from the list
            var newId = _availableIds[0];
            _availableIds.Remove(_availableIds[0]);
            return newId;
        }

        private void CreatePrefabForSelectedMesh()
        {
            var selectedAsset = Selection.activeObject;
            var customLog = CreatePrefab.CreatePrefabForMesh(selectedAsset, _activeRuleList);
            
            // Update the log list
            _logListView.Clear();

            foreach (var logEntry in customLog)
            {
                _listEntryVisualTreeAsset.CloneTree(_logListView);
                _listEntryButton = _root.Q<Button>(Constants.LabelButtonLogEntry);
                _listEntryButton.name = logEntry;
                _listEntryButton.text = logEntry;
                _logListView.Add(_listEntryButton);
            }
        }

        private void ClearLog()
        {
            _logListView.Clear();
        }

        private void UpdateActiveRuleListWithRulesListViewContents()
        {
            if (_rulesListView == null)
            {
                return;
            }
            
            _activeRuleList.Clear();

            var ruleCount = _rulesListView.childCount;
            for (var i = 0; i < ruleCount; i++)
            {
                var currentRule = new Rule();
                var currentVisualRule = _rulesListView[i];

                var visualRuleId = i;

                // Rule Id
                currentRule.RuleId = visualRuleId;
                
                // 'Name starts with' foldout
                currentRule.MeshNameStartsWith = new List<string>();
                var nameStartsWithFoldout = currentVisualRule.Q<Foldout>(Constants.LabelFoldoutIncludeNameStartsWith);
                var nameStartsWithFoldoutChildCount = nameStartsWithFoldout.childCount;
                for (var j = 1; j < nameStartsWithFoldoutChildCount; j++)
                {
                    var textField = (TextField) nameStartsWithFoldout.hierarchy.ElementAt(1).ElementAt(j).ElementAt(1);
                    currentRule.MeshNameStartsWith.Add(textField.text);
                }

                // 'Name contains' foldout
                currentRule.MeshNameContains = new List<string>();
                var nameContains = currentVisualRule.Q<Foldout>(Constants.LabelFoldoutIncludeNameContains);
                var nameContainsFoldoutChildCount = nameContains.childCount;
                for (var j = 1; j < nameContainsFoldoutChildCount; j++)
                {
                    var textField = (TextField) nameContains.hierarchy.ElementAt(1).ElementAt(j).ElementAt(1);
                    currentRule.MeshNameContains.Add(textField.text);
                }
                
                // 'Use Mesh Name'
                var isPrefabUseMeshName = currentVisualRule.Q<Toggle>(
                    Constants.LabelToggleUseMeshName).value;
                currentRule.IsPrefabUseMeshName = isPrefabUseMeshName;
                
                // 'Use Mesh Name, but Replace ... with ...'
                var isPrefabUseMeshNameReplace = currentVisualRule.Q<Toggle>(
                    Constants.LabelToggleUseMeshNameReplace).value;
                currentRule.IsPrefabUseMeshNameReplace = isPrefabUseMeshNameReplace;
                var prefabUseMeshNameReplaceSource = currentVisualRule.Q<TextField>(
                    Constants.LabelTextFieldUseMeshNameReplaceSource).text;
                currentRule.PrefabUseMeshNameReplaceSource = prefabUseMeshNameReplaceSource;
                var prefabUseMeshNameReplaceTarget = currentVisualRule.Q<TextField>(
                    Constants.LabelTextFieldUseMeshNameReplaceTarget).text;
                currentRule.PrefabUseMeshNameReplaceTarget = prefabUseMeshNameReplaceTarget;
                
                // 'Use Unique Name'
                var isPrefabUseUniqueName = currentVisualRule.Q<Toggle>(
                    Constants.LabelToggleUseUniqueName).value;
                currentRule.IsPrefabUseUniqueName = isPrefabUseUniqueName;
                var prefabUseUniqueNameTarget = currentVisualRule.Q<TextField>(
                    Constants.LabelTextFieldUseUniqueNameTarget).text;
                currentRule.PrefabUseUniqueNameTarget = prefabUseUniqueNameTarget;
                
                // 'Add Suffix'
                var isPrefabAddSuffix = currentVisualRule.Q<Toggle>(
                    Constants.LabelToggleAddSuffix).value;
                currentRule.IsPrefabAddSuffix = isPrefabAddSuffix;
                var prefabAddSuffixTarget = currentVisualRule.Q<TextField>(
                    Constants.LabelTextFieldAddSuffixTarget).text;
                currentRule.PrefabAddSuffixTarget = prefabAddSuffixTarget;
                
                // 'Create Material for the Mesh'
                var isMaterialCreateMaterialForMesh = currentVisualRule.Q<Toggle>(
                    Constants.LabelToggleCreateMaterialForMesh).value;
                currentRule.IsMaterialCreateMaterialForMesh = isMaterialCreateMaterialForMesh;
                
                // 'Give it this shader'
                var materialShaderTarget = (Shader) currentVisualRule.Q<ObjectField>(
                    Constants.LabelObjectFieldShader).value;
                currentRule.MaterialShaderTargetRelativePath = AssetDatabase.GetAssetPath(materialShaderTarget);
                
                // 'For naming, use <MeshName> + ... (Mat)
                var isMaterialMeshNamePlusSuffix = currentVisualRule.Q<Toggle>(
                    Constants.LabelToggleMaterialUseMeshNamePlusSuffix).value;
                currentRule.IsMaterialMeshNamePlusSuffix = isMaterialMeshNamePlusSuffix;
                var materialMeshNameSuffixTarget = currentVisualRule.Q<TextField>(
                    Constants.LabelTextFieldMaterialUseMeshNamePlusSuffixTarget).text;
                currentRule.MaterialMeshNameSuffixTarget = materialMeshNameSuffixTarget;
                
                // 'Shader Inputs and Equivalent Texture Suffixes Matchings'
                currentRule.MaterialShaderInputToTextureSuffixMapping = new Dictionary<string, string>();
                var materialShaderInputToTextureSuffixMapping = currentVisualRule.Q<Foldout>(
                    Constants.LabelFoldoutTextureInputs);
                var materialShaderInputToTextureSuffixMappingChildCount =
                    materialShaderInputToTextureSuffixMapping.childCount;
                for (var j = 1; j < materialShaderInputToTextureSuffixMappingChildCount; j++)
                {
                    var textFieldSource = (TextField) 
                        materialShaderInputToTextureSuffixMapping.hierarchy.ElementAt(1).ElementAt(j).ElementAt(1);
                    var textFieldTarget = (TextField) 
                        materialShaderInputToTextureSuffixMapping.hierarchy.ElementAt(1).ElementAt(j).ElementAt(3);
                    currentRule.MaterialShaderInputToTextureSuffixMapping.Add(textFieldSource.text, textFieldTarget.text);
                }
                
                // 'Texture extension is ..."
                var materialTextureExtension = currentVisualRule.Q<TextField>(
                    Constants.LabelTextFieldTextureExtensionTarget).text;
                currentRule.MaterialTextureExtension = materialTextureExtension;

                // 'Assign all textures that match the <MeshName> + <Suffix>
                var isMaterialAssignAllTexturesMatchMeshName = currentVisualRule.Q<Toggle>(
                    Constants.LabelToggleAssignAllTexturesToMaterial).value;
                currentRule.IsMaterialAssignAllTexturesMatchMeshName = isMaterialAssignAllTexturesMatchMeshName;
                
                // Assign the new material to the mesh
                var isMaterialAssignMaterialToMesh = currentVisualRule.Q<Toggle>(
                    Constants.LabelToggleAssignMaterialToMesh).value;
                currentRule.IsMaterialAssignMaterialToMesh = isMaterialAssignMaterialToMesh;
                
                // Add the new rule to the active rule list
                _activeRuleList.Add(currentRule);
            }
        }

        private void UpdateRulesListViewContentsWithActiveRuleList()
        {
            var activeRuleCount = _activeRuleList.Count;
            _rulesListView.Clear();

            for (var i=0; i<activeRuleCount; i++)
            {
                var currentRule = _activeRuleList[i];
                var newRule = GetNewRule();
                
                // Rule Id
                var visualRuleId = i;
                
                // 'Name starts with' foldout
                var nameStartsWithEntryCount = currentRule.MeshNameStartsWith.Count;
                var nameStartsWithFoldout = newRule.Q<Foldout>(
                    Constants.LabelFoldoutIncludeNameStartsWith);
                for(var j = 0; j < nameStartsWithEntryCount; j++)
                {
                    AddNewSingleEntryToFoldout(nameStartsWithFoldout, currentRule.MeshNameStartsWith[j]);
                }

                // 'Name contains' foldout
                var nameContainsEntryCount = currentRule.MeshNameContains.Count;
                var nameContainsFoldout = newRule.Q<Foldout>(
                    Constants.LabelFoldoutIncludeNameContains);
                for(var j = 0; j < nameContainsEntryCount; j++)
                {
                    AddNewSingleEntryToFoldout(nameContainsFoldout, currentRule.MeshNameContains[j]);
                }

                // 'Use Mesh Name'
                var isPrefabUseMeshName = newRule.Q<Toggle>(
                    Constants.LabelToggleUseMeshName);
                isPrefabUseMeshName.value = currentRule.IsPrefabUseMeshName;
                
                // 'Use Mesh Name, but Replace ... with ...'
                var isPrefabUseMeshNameReplace = newRule.Q<Toggle>(
                    Constants.LabelToggleUseMeshNameReplace);
                isPrefabUseMeshNameReplace.value = currentRule.IsPrefabUseMeshNameReplace;
                var isPrefabUseMeshNameReplaceSource = newRule.Q<TextField>(
                    Constants.LabelTextFieldUseMeshNameReplaceSource);
                isPrefabUseMeshNameReplaceSource.value = currentRule.PrefabUseMeshNameReplaceSource;
                var isPrefabUseMeshNameReplaceTarget = newRule.Q<TextField>(
                    Constants.LabelTextFieldUseMeshNameReplaceTarget);
                isPrefabUseMeshNameReplaceTarget.value = currentRule.PrefabUseMeshNameReplaceTarget;

                // 'Use Unique Name'
                var isPrefabUseUniqueName = newRule.Q<Toggle>(
                    Constants.LabelToggleUseUniqueName);
                isPrefabUseUniqueName.value = currentRule.IsPrefabUseUniqueName;
                var isPrefabUseUniqueNameTarget = newRule.Q<TextField>(
                    Constants.LabelTextFieldUseUniqueNameTarget);
                isPrefabUseUniqueNameTarget.value = currentRule.PrefabUseUniqueNameTarget;

                // 'Add Suffix'
                var isPrefabAddSuffix = newRule.Q<Toggle>(
                    Constants.LabelToggleAddSuffix);
                isPrefabAddSuffix.value = currentRule.IsPrefabAddSuffix;
                var isPrefabAddSuffixTarget = newRule.Q<TextField>(
                    Constants.LabelTextFieldAddSuffixTarget);
                isPrefabAddSuffixTarget.value = currentRule.PrefabAddSuffixTarget;
                
                // 'Create Material for the Mesh'
                var isMaterialCreateMaterialForMesh = newRule.Q<Toggle>(
                    Constants.LabelToggleCreateMaterialForMesh);
                isMaterialCreateMaterialForMesh.value = currentRule.IsMaterialCreateMaterialForMesh;

                // 'Give it this shader'
                var materialShaderTarget = newRule.Q<ObjectField>(
                    Constants.LabelObjectFieldShader);
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(currentRule.MaterialShaderTargetRelativePath);
                materialShaderTarget.value = shader;

                // 'For naming, use <MeshName> + ... (Mat)
                var isMaterialMeshNamePlusSuffix = newRule.Q<Toggle>(
                    Constants.LabelToggleMaterialUseMeshNamePlusSuffix);
                isMaterialMeshNamePlusSuffix.value = currentRule.IsMaterialMeshNamePlusSuffix;
                var materialMeshNameSuffixTarget = newRule.Q<TextField>(
                    Constants.LabelTextFieldMaterialUseMeshNamePlusSuffixTarget);
                materialMeshNameSuffixTarget.value = currentRule.MaterialMeshNameSuffixTarget;
                
                var materialShaderInputToTextureSuffixMapping = newRule.Q<Foldout>(
                    Constants.LabelFoldoutTextureInputs);
                materialShaderInputToTextureSuffixMapping.Clear();
                foreach(var mapping in currentRule.MaterialShaderInputToTextureSuffixMapping)
                {
                    AddNewDoubleEntryToFoldout(
                        materialShaderInputToTextureSuffixMapping, 
                        mapping.Key,
                        mapping.Value);
                }
                
                // 'Texture extension is ..."
                var materialTextureExtension = newRule.Q<TextField>(
                    Constants.LabelTextFieldTextureExtensionTarget);
                materialTextureExtension.value = currentRule.MaterialTextureExtension;

                // 'Assign all textures that match the <MeshName> + <Suffix>
                var isMaterialAssignAllTexturesMatchMeshName = newRule.Q<Toggle>(
                    Constants.LabelToggleAssignAllTexturesToMaterial);
                isMaterialAssignAllTexturesMatchMeshName.value = currentRule.IsMaterialAssignAllTexturesMatchMeshName;
                
                // Assign the new material to the mesh
                var isMaterialAssignMaterialToMesh = newRule.Q<Toggle>(
                    Constants.LabelToggleAssignMaterialToMesh);
                isMaterialAssignMaterialToMesh.value = currentRule.IsMaterialAssignMaterialToMesh;
            }
        }
    }                                                                                                                                        
}
