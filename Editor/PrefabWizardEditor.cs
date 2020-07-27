using System;
using System.Collections.Generic;
using System.IO;using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityPrefabWizard.AssetUtilities;
using UnityPrefabWizard.SystemUtilities;

namespace UnityPrefabWizard.Editor
{
    public class PrefabWizardEditor : EditorWindow
    {
        // private string _version = "v.0.0.1.20200710";

        private const int WindowWidth = 500;
        private const int WindowHeight = 800;

        private const string PrefabExtension = ".prefab";
        private const string ErrorTitleError = "Error";
        private const string ErrorBodySelectOneMesh = "Please select one mesh in the project window!";
        private const string ErrorTextureDoesNotExist = "The expected texture does not exist in the project: ";
        private const string ErrorButtonOk = "OK";
        
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

        [MenuItem("Art Tools/Launch Prefab Wizard")]                                                                                     
        public static void ShowWindow()                                                                                                      
        {                                                                                                                                    
            // Opens the window, otherwise focuses it if it’s already open.                                                                  
            var window = GetWindow<PrefabWizardEditor>();

            // Adds a title to the window.                                                                                                   
            window.titleContent = new GUIContent("Prefab Wizard");                                                                
                                                                                                                                             
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
            _contents = Resources.Load<VisualTreeAsset>("CS_PrefabWizard");
            _contents.CloneTree(_root);

            _rulesListView = _root.Q<ListView>("LV_RulesList");
            
            // Single Rule Reference
            _singleRuleVisualTree = Resources.Load<VisualTreeAsset>("CS_SingleRule");

            // Load Rules Button
            _loadRules = _root.Q<Button>("BT_LoadRules");
            _loadRules.clickable.clicked += LoadRules;

            // Save Rules Button
            _saveRules = _root.Q<Button>("BT_SaveRules");
            _saveRules.clickable.clicked += SaveRules;
            
            // Add Rule Button
            _addRule = _root.Q<Button>("BT_AddRule");
            _addRule.clickable.clicked += AddNewRule;
            
            // Clear Rules Button
            _clearRules = _root.Q<Button>("BT_ClearRules");
            _clearRules.clickable.clicked += ClearRules;
            
            // Create Prefab Button
            _createPrefab = _root.Q<Button>("BT_CreatePrefab");
            _createPrefab.clickable.clicked += CreatePrefabForSelectedMesh;
            
            // Log List View
            _logListView = _root.Q<ListView>("LV_Log");
            _listEntryVisualTreeAsset = Resources.Load<VisualTreeAsset>("CS_LogEntry");
            _logListView.Clear();
            
            // ClearLog
            _clearLog = _root.Q<Button>("BT_ClearLog");
            _clearLog.clickable.clicked += ClearLog;
        }

        private void LoadRules()
        {
            var rulesPath = EditorUtility.OpenFilePanel("", "", "json");
            if (String.IsNullOrWhiteSpace(rulesPath))
            {
                
                return;
            }
            
            _activeRuleList = PrefabWizard.GetRules(rulesPath);
        }

        private void SaveRules()
        {
            var rulesPath = EditorUtility.SaveFilePanel(
                "", "", "rules", "json");

            if (String.IsNullOrWhiteSpace(rulesPath))
            {
                return;
            }
            
            PrefabWizard.SetRules(_activeRuleList, rulesPath);
            AssetDatabase.Refresh();
            
            // Update the list
            _logListView.Clear(); 
            _listEntryVisualTreeAsset.CloneTree(_logListView);
            _listEntryButton = _root.Q<Button>("BT_LogEntry");
            _listEntryButton.name += "SaveLog";
            _listEntryButton.text = "Successfully saved rules to path: " + rulesPath;
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
            
            var newRuleVisualElement = _root.Q<VisualElement>("VE_SingleRule");

            newRuleVisualElement.name += id.ToString();
            newRuleVisualElement.style.borderTopColor = new StyleColor(randomColor);
            newRuleVisualElement.style.borderRightColor = new StyleColor(randomColor);
            newRuleVisualElement.style.borderBottomColor = new StyleColor(randomColor);
            newRuleVisualElement.style.borderLeftColor = new StyleColor(randomColor);
            
            // 'Name starts with' foldout
            var nameStartsWithFoldout = _root.Q<Foldout>("FO_IncludeNameStartsWith");
            nameStartsWithFoldout.name += id.ToString();
            
            // Add new field Button for 'name starts with'
            var nameStartsWithButton = _root.Q<Button>("BT_IncludeNameStartsWith");
            nameStartsWithButton.name += id.ToString();
            nameStartsWithButton.clickable.clicked += () => AddNewSingleEntryToFoldout(nameStartsWithFoldout);
            
            // 'Name contains' foldout
            var nameContainsFoldout = _root.Q<Foldout>("FO_IncludeNameContains");
            nameContainsFoldout.name += id.ToString();

            // Add new field Button for 'name contains'
            var nameContainsButton = _root.Q<Button>("BT_IncludeNameContains");
            nameContainsButton.name += id.ToString();
            nameContainsButton.clickable.clicked += () => AddNewSingleEntryToFoldout(nameContainsFoldout);
            
            // Shader slot
            var shaderObjectField = _root.Q<ObjectField>("OF_Shader");
            shaderObjectField.name += id.ToString();
            shaderObjectField.objectType = typeof(Shader);
            
            // 'Texture Input' foldout
            var textureInputFoldout = _root.Q<Foldout>("FO_TextureInputs");
            textureInputFoldout.name += id.ToString();
            
            // Default Values for 'Texture Input' foldout
            foreach (var mapping in _defaultShaderPropertyToTextureSuffixMatching)
            {
                AddNewDoubleEntryToFoldout(
                    textureInputFoldout,
                    mapping.Key,
                    mapping.Value);
            }

            // Add new field Button for 'texture inputs'
            var textureInputsButton = _root.Q<Button>("BT_AddTextureInputMatching");
            textureInputsButton.name += id.ToString();
            textureInputsButton.clickable.clicked += () => AddNewDoubleEntryToFoldout(textureInputFoldout);

            // Remove Button
            var removeButton = _root.Q<Button>("BT_Remove");
            removeButton.name += id.ToString();
            removeButton.clickable.clicked += () => RemoveRule(newRuleVisualElement, id);
            removeButton.style.backgroundColor = randomColor;
            removeButton.style.color = inverseRandomColor;
            
            // Rule count Label
            var labelRuleNumber = _root.Q<Label>("LB_Rule");
            labelRuleNumber.name += id.ToString();
            labelRuleNumber.text += id.ToString();
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
                EditorUtility.DisplayDialog(ErrorTitleError, ErrorBodySelectOneMesh, ErrorButtonOk);
                return;
            }
            
            var selectedAssetPath = AssetDatabase.GetAssetPath(selectedAsset);
            if (String.IsNullOrWhiteSpace(selectedAssetPath))
            {
                EditorUtility.DisplayDialog(ErrorTitleError, ErrorBodySelectOneMesh, ErrorButtonOk);
                return;
            }
            
            // Check if the asset is a model
            var model = (Mesh) AssetDatabase.LoadAssetAtPath(selectedAssetPath, typeof(Mesh));
            if (model == null)
            {
                EditorUtility.DisplayDialog(ErrorTitleError, ErrorBodySelectOneMesh, ErrorButtonOk);
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
    }                                                                                                                                        
}
