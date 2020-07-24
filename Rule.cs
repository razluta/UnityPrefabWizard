using System.Collections.Generic;
using UnityEngine;

namespace UnityPrefabWizard
{
    public class Rule
    {
        // ID
        public int RuleId { get; set; }
        
        // Mesh Rules
        public List<string> MeshNameStartsWith { get; set; }
        public List<string> MeshNameContains { get; set; }

        // Prefab Rules
        public bool IsPrefabUseMeshName { get; set; }
        
        public bool IsPrefabUseMeshNameReplace { get; set; }
        public string PrefabUseMeshNameReplaceSource { get; set; }
        public string PrefabUseMeshNameReplaceTarget { get; set; }
        
        public bool IsPrefabUseUniqueName { get; set; }
        public string PrefabUseUniqueNameTarget { get; set; }
        
        public bool IsPrefabAddSuffix { get; set; }
        public string PrefabAddSuffixTarget { get; set; }
        
        // Material Rules
        public bool IsMaterialCreateMaterialForMesh { get; set; }
        
        public bool IsMaterialMeshNamePlusSuffix { get; set; }
        public string MaterialMeshNameSuffixTarget { get; set; }
        
        public Shader MaterialShaderTarget { get; set; }
        
        public List<Dictionary<string, string>> MaterialShaderInputToTextureSuffixMapping { get; set; }
        
        public bool IsMaterialAssignAllTexturesMatchMeshName { get; set; }
        public bool IsMaterialAssignMaterialToMesh { get; set; }
    }
}