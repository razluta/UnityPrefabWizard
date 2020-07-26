using System;
using System.Collections.Generic;
using UnityPrefabWizard.SystemUtilities;

namespace UnityPrefabWizard
{
    public static class PrefabWizard
    {
        public static List<Rule> GetRules(string path=null)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                return GetDefaultRules();
            }
            
            var rules = JsonUtilities.GetData(path);
            return rules;
        }
        
        public static void SetRules(List<Rule> rules, string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                return;
            }
            
            JsonUtilities.SetData(rules, path); 
        }

        private static List<Rule> GetDefaultRules()
        {
            var rules = new List<Rule>();
            rules.Add(
                new Rule()
                {
                    
                });

            return rules;
        }
    }
}