using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;

namespace UnityPrefabWizard.SystemUtilities
{
    public static class JsonUtilities
    {
        public static List<Rule> GetData(string path)
        {
            var file = File.OpenText(path);
            var serializers = new JsonSerializer();
            var rules = (List<Rule>) serializers.Deserialize(file, typeof(List<Rule>));
            return rules;
        }
        
        public static void SetData(List<Rule> rules, string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(rules));
        }
    }
}