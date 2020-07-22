using System.Collections.Generic;
using UnityEditor;
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

        private VisualElement _root;
        private VisualTreeAsset _contents;

        private ListView _rulesListView;
        private VisualTreeAsset _singleRuleVisualTree;
        private VisualElement _singleRuleVisualElement;

        private int _ruleCount;
        private List<int> _availableIds;
        private Button _addRule;
        private Button _clearRules;

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
            
            // Save Rules Button
            
            // Add Rule Button
            _addRule = _root.Q<Button>("BT_AddRule");
            _addRule.clickable.clicked += AddNewRule;
            
            // Clear Rules Button
            _clearRules = _root.Q<Button>("BT_ClearRules");
            _clearRules.clickable.clicked += ClearRules;
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
            nameStartsWithButton.clickable.clicked += () => AddNewEntryToFoldout(nameStartsWithFoldout);
            
            // 'Name contains' foldout
            var nameContainsFoldout = _root.Q<Foldout>("FO_IncludeNameContains");
            nameContainsFoldout.name += id.ToString();

            // Add new field Button for 'name contains'
            var nameContainsButton = _root.Q<Button>("BT_IncludeNameContains");
            nameContainsButton.name += id.ToString();
            nameContainsButton.clickable.clicked += () => AddNewEntryToFoldout(nameContainsFoldout);
            
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

        private void AddNewEntryToFoldout(Foldout foldout)
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
    }                                                                                                                                        
}
