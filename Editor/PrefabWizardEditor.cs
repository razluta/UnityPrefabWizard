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
        private const int WindowHeight = 700;

        private VisualElement _root;
        private VisualTreeAsset _contents;

        private ListView _rulesListView;
        private VisualTreeAsset _singleRuleVisualTree;
        private VisualElement _singleRuleVisualElement;

        private int _ruleCount;
        private Button _addRule;

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
            _ruleCount = 0;
            
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
        }

        private void AddNewRule()
        {
            _singleRuleVisualTree.CloneTree(
                _rulesListView.hierarchy.ElementAt(0
                ).hierarchy.ElementAt(0
                ).hierarchy.ElementAt(0));
            var randomColor = GeneralUtilities.GetRandomColor();
            var newRuleVisualElement = _root.Q<VisualElement>("VE_SingleRule");
            
            newRuleVisualElement.name += _ruleCount.ToString();
            newRuleVisualElement.style.borderTopColor = new StyleColor(randomColor);
            newRuleVisualElement.style.borderRightColor = new StyleColor(randomColor);
            newRuleVisualElement.style.borderBottomColor = new StyleColor(randomColor);
            newRuleVisualElement.style.borderLeftColor = new StyleColor(randomColor);

            var removeButton = _root.Q<Button>("BT_Remove");
            removeButton.name += _ruleCount.ToString();
            removeButton.clickable.clicked += () => RemoveRule(newRuleVisualElement);
            removeButton.style.backgroundColor = randomColor;
            
            _ruleCount++;
        }

        private void RemoveRule(VisualElement singleRule)
        {
            _rulesListView.Remove(singleRule);
        }
    }                                                                                                                                        
}
