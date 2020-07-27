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
        // private string _version = "v.0.0.1.20200710";

        private const int WindowWidth = 500;
        private const int WindowHeight = 800;
        private const string WindowName = "Prefab Wizard";
        private const string WindowMenuPath = "Art Tools/" + WindowName;
        
        private const string PrefabExtension = ".prefab";
        private const string JsonExtension = "json";
        private const string DefaultRulesFileName = "rules";

        private const string LabelUxmlMainPrefabWizard = "CS_PrefabWizard";
        private const string LabelListViewRulesList = "LV_RulesList";
        private const string LabelUxmlSingleRule = "CS_SingleRule";
        private const string LabelVisualElementSingleRule = "VE_SingleRule";
        private const string LabelButtonLoadRules = "BT_LoadRules";
        private const string LabelButtonSaveRules = "BT_SaveRules";
        private const string LabelButtonAddRule = "BT_AddRule";
        private const string LabelButtonClearRules = "BT_ClearRules";
        private const string LabelButtonCreatePrefab = "BT_CreatePrefab";
        private const string LabelListViewLog = "LV_Log";
        private const string LabelUxmlLogEntry = "CS_LogEntry";
        private const string LabelButtonLogEntry = "BT_LogEntry";
        private const string LabelButtonClearLog = "BT_ClearLog";

        private const string LabelButtonNameStartsWith = "BT_IncludeNameStartsWith";
        private const string LabelFoldoutIncludeNameStartsWith = "FO_IncludeNameStartsWith";
        private const string LabelButtonNameContains = "BT_IncludeNameContains";
        private const string LabelFoldoutIncludeNameContains = "FO_IncludeNameContains";
        private const string LabelToggleUseMeshName = "TG_UseMeshName";
        private const string LabelToggleUseMeshNameReplace = "TG_UseMeshNameReplace";
        private const string LabelTextFieldUseMeshNameReplaceSource = "TF_UseMeshNameReplaceSource";
        private const string LabelTextFieldUseMeshNameReplaceTarget = "TF_UseMeshNameReplaceTarget";
        private const string LabelToggleUseUniqueName = "TG_UseUniqueName";
        private const string LabelTextFieldUseUniqueNameTarget = "TF_UseUniqueNameTarget";
        private const string LabelToggleAddSuffix = "TG_AddSuffix";
        private const string LabelTextFieldAddSuffixTarget = "TF_AddSuffixTarget";
        private const string LabelToggleCreateMaterialForMesh = "TG_CreateMaterialForMesh";
        private const string LabelObjectFieldShader = "OF_Shader";
        private const string LabelToggleMaterialUseMeshName = "TG_MaterialUseMeshName";
        private const string LabelTextFieldMaterialUseMeshNameTarget = "TF_MaterialUseMeshNameTarget";
        private const string LabelFoldoutTextureInputs = "FO_TextureInputs";
        private const string LabelTextFieldTextureExtensionTarget = "TF_TextureExtensionTarget";
        private const string LabelToggleAssignAllTexturesToMaterial = "TG_AssignAllTexturesToMaterial";
        private const string LabelToggleAssignMaterialToMesh = "TG_AssignMaterialToMesh";

        private const string LabelButtonAddTextureInputMatching = "BT_AddTextureInputMatching";
        private const string LabelButtonRemove = "BT_Remove";
        
        private const string TitleError = "Error";
        private const string MessageErrorBodySelectOneMesh = "Please select one mesh in the project window!";
        private const string MessageErrorTextureDoesNotExist = "The expected texture does not exist in the project: ";
        private const string LabelButtonErrorOk = "OK";
        private const string MessageSuccessfullySavedRules = "Successfully saved rules to path: ";

        private readonly Dictionary<string, string> _defaultShaderPropertyToTextureSuffixMatching = 
            new Dictionary<string, string>()
        {
            {"_MainTex", "_diff"}, // Albedo or diffuse texture
            {"_SpecGlossMap", "_spec"}, // Specular texture
            {"_BumpMap", "_norm"}, // Normal map texture
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

        private List<Rule> _activeRuleList;

        [MenuItem(WindowMenuPath)]                                                                                     
        public static void ShowWindow()                                                                                                      
        {                                                                                                                                    
            // Opens the window, otherwise focuses it if it’s already open.                                                                  
            var window = GetWindow<PrefabWizardEditor>();

            // Adds a title to the window.                                                                                                   
            window.titleContent = new GUIContent(WindowName);                                                                
                                                                                                                                             
            // Sets a minimum size to the window.                                                                                            
            window.minSize = new Vector2(WindowWidth, WindowHeight);
        }                                                                                                                                    
                                                                                                                                             
        private void OnEnable()
        {
            _ruleCount = -1;
            _activeRuleList = new List<Rule>();
            _availableIds = new List<int>();
            
            // Reference to the root of the window.                                                                                          
            _root = rootVisualElement;
            
            // Instantiate contents
            _contents = Resources.Load<VisualTreeAsset>(LabelUxmlMainPrefabWizard);
            _contents.CloneTree(_root);

            _rulesListView = _root.Q<ListView>(LabelListViewRulesList);
            
            // Single Rule Reference
            _singleRuleVisualTree = Resources.Load<VisualTreeAsset>(LabelUxmlSingleRule);

            // Load Rules Button
            _loadRules = _root.Q<Button>(LabelButtonLoadRules);
            _loadRules.clickable.clicked += LoadRules;

            // Save Rules Button
            _saveRules = _root.Q<Button>(LabelButtonSaveRules);
            _saveRules.clickable.clicked += SaveRules;
            
            // Add Rule Button
            _addRule = _root.Q<Button>(LabelButtonAddRule);
            _addRule.clickable.clicked += AddNewRule;
            
            // Clear Rules Button
            _clearRules = _root.Q<Button>(LabelButtonClearRules);
            _clearRules.clickable.clicked += ClearRules;
            
            // Create Prefab Button
            _createPrefab = _root.Q<Button>(LabelButtonCreatePrefab);
            _createPrefab.clickable.clicked += CreatePrefabForSelectedMesh;
            
            // Log List View
            _logListView = _root.Q<ListView>(LabelListViewLog);
            _listEntryVisualTreeAsset = Resources.Load<VisualTreeAsset>(LabelUxmlLogEntry);
            _logListView.Clear();
            
            // ClearLog
            _clearLog = _root.Q<Button>(LabelButtonClearLog);
            _clearLog.clickable.clicked += ClearLog;
        }

        private void LoadRules()
        {
            var rulesPath = EditorUtility.OpenFilePanel("", "", JsonExtension);
            if (String.IsNullOrWhiteSpace(rulesPath))
            {
                
                return;
            }
            
            _activeRuleList = PrefabWizard.GetRules(rulesPath);
        }

        private void SaveRules()
        {
            var rulesPath = EditorUtility.SaveFilePanel(
                "", "", DefaultRulesFileName, JsonExtension);

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
            _listEntryButton = _root.Q<Button>(LabelButtonLogEntry);
            _listEntryButton.text = MessageSuccessfullySavedRules + rulesPath;
            _logListView.Add(_listEntryButton);
        }

        private void AddNewRule()
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
            
            var newRuleVisualElement = _root.Q<VisualElement>(LabelVisualElementSingleRule);
            
            newRuleVisualElement.style.borderTopColor = new StyleColor(randomColor);
            newRuleVisualElement.style.borderRightColor = new StyleColor(randomColor);
            newRuleVisualElement.style.borderBottomColor = new StyleColor(randomColor);
            newRuleVisualElement.style.borderLeftColor = new StyleColor(randomColor);
            
            // 'Name starts with' foldout
            var nameStartsWithFoldout = newRuleVisualElement.Q<Foldout>(LabelFoldoutIncludeNameStartsWith);

            // Add new field Button for 'name starts with'
            var nameStartsWithButton = newRuleVisualElement.Q<Button>(LabelButtonNameStartsWith);
            nameStartsWithButton.clickable.clicked += () => AddNewSingleEntryToFoldout(nameStartsWithFoldout);
            
            // 'Name contains' foldout
            var nameContainsFoldout = newRuleVisualElement.Q<Foldout>(LabelFoldoutIncludeNameContains);

            // Add new field Button for 'name contains'
            var nameContainsButton = newRuleVisualElement.Q<Button>(LabelButtonNameContains);
            nameContainsButton.clickable.clicked += () => AddNewSingleEntryToFoldout(nameContainsFoldout);
            
            // Shader slot
            var shaderObjectField = newRuleVisualElement.Q<ObjectField>(LabelObjectFieldShader);
            shaderObjectField.objectType = typeof(Shader);
            
            // 'Texture Input' foldout
            var textureInputFoldout = newRuleVisualElement.Q<Foldout>(LabelFoldoutTextureInputs);

            // Default Values for 'Texture Input' foldout
            foreach (var mapping in _defaultShaderPropertyToTextureSuffixMatching)
            {
                AddNewDoubleEntryToFoldout(
                    textureInputFoldout,
                    mapping.Key,
                    mapping.Value);
            }

            // Add new field Button for 'texture inputs'
            var textureInputsButton = newRuleVisualElement.Q<Button>(LabelButtonAddTextureInputMatching);
            textureInputsButton.clickable.clicked += () => AddNewDoubleEntryToFoldout(textureInputFoldout);

            // Remove Button
            var removeButton = newRuleVisualElement.Q<Button>(LabelButtonRemove);
            removeButton.clickable.clicked += () => RemoveRule(newRuleVisualElement, id);
            removeButton.style.backgroundColor = randomColor;
            removeButton.style.color = inverseRandomColor;
        }

        private void AddNewSingleEntryToFoldout(Foldout foldout)
        {
            var newIncludeNameStartsWithVisualElement = new VisualElement();
            newIncludeNameStartsWithVisualElement.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            
            var newIncludeNameStartsWithTextField = new TextField();
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
            // Obtain the selected model
            var selectedAsset = Selection.activeObject;

            if (selectedAsset == null)
            {
                EditorUtility.DisplayDialog(TitleError, MessageErrorBodySelectOneMesh, LabelButtonErrorOk);
                return;
            }
            
            var selectedAssetPath = AssetDatabase.GetAssetPath(selectedAsset);
            if (String.IsNullOrWhiteSpace(selectedAssetPath))
            {
                EditorUtility.DisplayDialog(TitleError, MessageErrorBodySelectOneMesh, LabelButtonErrorOk);
                return;
            }
            
            // Check if the asset is a model
            var model = (Mesh) AssetDatabase.LoadAssetAtPath(selectedAssetPath, typeof(Mesh));
            if (model == null)
            {
                EditorUtility.DisplayDialog(TitleError, MessageErrorBodySelectOneMesh, LabelButtonErrorOk);
                return;
            }
            
            var assetDirectoryPath = Path.GetDirectoryName(selectedAssetPath);
            if (String.IsNullOrWhiteSpace(assetDirectoryPath))
            {
                return;
            }
            
            // Parse every rule in the rule list
            foreach (var rule in _activeRuleList)
            {
                // Instantiate the model in the current scene and name it in preparation for creating the prefab out of it
                var modelInScene = (GameObject) Instantiate(selectedAsset);

                // 'use mesh name'
                if (rule.IsPrefabUseMeshName)
                {
                    modelInScene.name = selectedAsset.name;
                }
                // 'use mesh name, but replace *** with ***
                else if (rule.IsPrefabUseMeshNameReplace)
                {
                    if (!String.IsNullOrWhiteSpace(rule.PrefabUseMeshNameReplaceSource) &&
                        !String.IsNullOrWhiteSpace(rule.PrefabUseMeshNameReplaceTarget))
                    {
                        modelInScene.name = selectedAsset.name.Replace(
                            rule.PrefabUseMeshNameReplaceSource,
                            rule.PrefabUseMeshNameReplaceTarget);
                    }
                }
                // 'use unique name'
                else if (rule.IsPrefabUseUniqueName)
                {
                    if (!String.IsNullOrWhiteSpace(rule.PrefabUseUniqueNameTarget))
                    {
                        modelInScene.name = rule.PrefabUseUniqueNameTarget;
                    }
                }

                // 'add suffix'
                if (rule.IsPrefabAddSuffix)
                {
                    if (!String.IsNullOrWhiteSpace(rule.PrefabAddSuffixTarget))
                    {
                        modelInScene.name += rule.PrefabAddSuffixTarget;
                    }
                }
                
                // 'create a material for the mesh'
                // 'give the material this shader'
                if (rule.MaterialShaderTarget)
                {
                    var material = new Material(rule.MaterialShaderTarget);
                    material.name = selectedAsset.name;

                    // 'for naming, use <MeshName> + custom string'
                    if (rule.IsMaterialMeshNamePlusSuffix)
                    {
                        if (!String.IsNullOrWhiteSpace(rule.MaterialMeshNameSuffixTarget))
                        {
                            material.name = selectedAsset.name + rule.MaterialMeshNameSuffixTarget;
                        }
                    }
                    
                    // 'shader inputs and equivalent texture suffixes matchings'
                    foreach (var mapping in rule.MaterialShaderInputToTextureSuffixMapping)
                    {
                        // assign all textures that match the <MeshName>
                        var shaderProperty = mapping.Key;
                        var textureNameSuffix = mapping.Value;

                        var expectedTexturePath = Path.Combine(
                            assetDirectoryPath,
                            selectedAsset.name + textureNameSuffix + rule.MaterialTextureExtension);
                        
                        var texture = (Texture2D) AssetDatabase.LoadAssetAtPath(expectedTexturePath, typeof(Texture2D));
                        
                        // Exit early if the texture does not exist in the project
                        if (!texture)
                        {
                            // Debug.Log(ErrorTextureDoesNotExist + expectedTexturePath);
                            continue;
                        }
                        
                        // Assign texture to the appropriate material slot
                        material.SetTexture(shaderProperty, texture);
                    }
                    
                    var renderer = modelInScene.GetComponent<Renderer>();
                    
                    // Save the material to the same folder
                    AssetDatabase.CreateAsset(material, 
                        Path.Combine(assetDirectoryPath, material.name + rule.MaterialMeshNameSuffixTarget));

                    // Apply the material to the sub-mesh
                    renderer.material = material;

                    // Write all unsaved assets to disk
                    AssetDatabase.SaveAssets();
                }
                
                // Create a prefab
                PrefabUtility.SaveAsPrefabAssetAndConnect(
                    modelInScene,
                    Path.Combine(assetDirectoryPath, modelInScene.name + PrefabExtension),
                    InteractionMode.UserAction);
                
                // Cleanup - remove asset from scene
                DestroyImmediate(modelInScene);
            }
            
        }

        private void ClearLog()
        {
            _logListView.Clear();
        }

        private void UpdateActiveRuleListWithRulesListViewContents()
        {
            // source: _rulesListView 
            // target: _activeRuleList
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
                var nameStartsWithFoldout = currentVisualRule.Q<Foldout>(LabelFoldoutIncludeNameStartsWith);
                var nameStartsWithFoldoutChildCount = nameStartsWithFoldout.childCount;
                for (var j = 1; j < nameStartsWithFoldoutChildCount; j++)
                {
                    var textField = (TextField) nameStartsWithFoldout.hierarchy.ElementAt(1).ElementAt(j).ElementAt(1);
                    currentRule.MeshNameStartsWith.Add(textField.text);
                }

                // 'Name contains' foldout
                currentRule.MeshNameContains = new List<string>();
                var nameContains = currentVisualRule.Q<Foldout>(LabelFoldoutIncludeNameContains);
                var nameContainsFoldoutChildCount = nameContains.childCount;
                for (var j = 1; j < nameContainsFoldoutChildCount; j++)
                {
                    var textField = (TextField) nameContains.hierarchy.ElementAt(1).ElementAt(j).ElementAt(1);
                    currentRule.MeshNameContains.Add(textField.text);
                }
                
                // 'Use Mesh Name'
                var isPrefabUseMeshName = currentVisualRule.Q<Toggle>(LabelToggleUseMeshName).value;
                currentRule.IsPrefabUseMeshName = isPrefabUseMeshName;
                
                // 'Use Mesh Name, but Replace ... with ...'
                var isPrefabUseMeshNameReplace = currentVisualRule.Q<Toggle>(LabelToggleUseMeshNameReplace).value;
                currentRule.IsPrefabUseMeshNameReplace = isPrefabUseMeshNameReplace;
                var prefabUseMeshNameReplaceSource = currentVisualRule.Q<TextField>(LabelTextFieldUseMeshNameReplaceSource).text;
                currentRule.PrefabUseMeshNameReplaceSource = prefabUseMeshNameReplaceSource;
                var prefabUseMeshNameReplaceTarget = currentVisualRule.Q<TextField>(LabelTextFieldUseMeshNameReplaceTarget).text;
                currentRule.PrefabUseMeshNameReplaceTarget = prefabUseMeshNameReplaceTarget;
                
                // 'Use Unique Name'
                var isPrefabUseUniqueName = currentVisualRule.Q<Toggle>(LabelToggleUseUniqueName).value;
                currentRule.IsPrefabUseUniqueName = isPrefabUseUniqueName;
                var prefabUseUniqueNameTarget = currentVisualRule.Q<TextField>(LabelTextFieldUseUniqueNameTarget).text;
                currentRule.PrefabUseUniqueNameTarget = prefabUseUniqueNameTarget;
                
                // 'Add Suffix'
                
                
                
                
                
                
                
                
                
                
                currentRule.IsPrefabAddSuffix = false;
                currentRule.PrefabAddSuffixTarget = "";
                currentRule.IsMaterialCreateMaterialForMesh = true;
                currentRule.MaterialShaderTarget = null;
                currentRule.IsMaterialMeshNamePlusSuffix = false;
                currentRule.MaterialMeshNameSuffixTarget = "";
                currentRule.MaterialShaderInputToTextureSuffixMapping = new Dictionary<string, string>()
                {
                    {" ", " "}
                };
                currentRule.MaterialTextureExtension = "";
                currentRule.IsMaterialAssignAllTexturesMatchMeshName = false;
                currentRule.IsMaterialAssignMaterialToMesh = false;
                
                _activeRuleList.Add(currentRule);
            }
            
        }
    }                                                                                                                                        
}
