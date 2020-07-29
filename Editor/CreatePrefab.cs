using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityPrefabWizard.Editor
{
    public class CreatePrefab : UnityEditor.Editor
    {
        public static List<string> CreatePrefabForMesh(Object asset, List<Rule> rules)
        {
            var customLogger = new List<string>();
            
            if (asset == null)
            {
                EditorUtility.DisplayDialog(
                    Constants.TitleError, 
                    Constants.MessageErrorBodySelectOneMesh, 
                    Constants.LabelButtonErrorOk);
                customLogger.Add(Constants.MessageErrorBodySelectOneMesh);
                return customLogger;
            }
            
            var selectedAssetPath = AssetDatabase.GetAssetPath(asset);
            if (String.IsNullOrWhiteSpace(selectedAssetPath))
            {
                EditorUtility.DisplayDialog(
                    Constants.TitleError, 
                    Constants.MessageErrorBodySelectOneMesh, 
                    Constants.LabelButtonErrorOk);
                customLogger.Add(Constants.MessageErrorBodySelectOneMesh);
                return customLogger;
            }
            
            // Check if the asset is a model
            var model = (Mesh) AssetDatabase.LoadAssetAtPath(selectedAssetPath, typeof(Mesh));
            if (model == null)
            {
                EditorUtility.DisplayDialog(
                    Constants.TitleError, 
                    Constants.MessageErrorBodySelectOneMesh, 
                    Constants.LabelButtonErrorOk);
                customLogger.Add(Constants.MessageErrorBodySelectOneMesh);
                return customLogger;
            }

            var assetDirectoryPath = Path.GetDirectoryName(selectedAssetPath);
            if (String.IsNullOrWhiteSpace(assetDirectoryPath))
            {
                return customLogger;
            }
            
            // Parse every rule in the rule list
            foreach (var rule in rules)
            {
                var hasMetCondition = false;
                foreach (var nameStartsWithEntry in rule.MeshNameStartsWith)
                {
                    if (asset.name.StartsWith(nameStartsWithEntry))
                    {
                        hasMetCondition = true;
                    }
                }
                
                foreach (var nameContainsEntry in rule.MeshNameContains)
                {
                    if (asset.name.Contains(nameContainsEntry))
                    {
                        hasMetCondition = true;
                    }
                }

                if (!hasMetCondition)
                {
                    continue;
                }
                
                customLogger.Add(Constants.MessageRuleMatch);
                
                // Instantiate the model in the current scene and name it in preparation for creating the prefab out of it
                var modelInScene = (GameObject) Instantiate(asset);
                modelInScene.name = asset.name;

                // 'use mesh name'
                if (rule.IsPrefabUseMeshName)
                {
                    modelInScene.name = asset.name;
                }
                // 'use mesh name, but replace *** with ***
                else if (rule.IsPrefabUseMeshNameReplace)
                {
                    if (!String.IsNullOrWhiteSpace(rule.PrefabUseMeshNameReplaceSource) &&
                        !String.IsNullOrWhiteSpace(rule.PrefabUseMeshNameReplaceTarget))
                    {
                        modelInScene.name = asset.name.Replace(
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
                customLogger.Add(Constants.MessageRenamedAssetUsing + modelInScene.name);

                var newModelName = modelInScene.name;
                // 'add suffix'
                if (rule.IsPrefabAddSuffix)
                {
                    if (!String.IsNullOrWhiteSpace(rule.PrefabAddSuffixTarget))
                    {
                        modelInScene.name += rule.PrefabAddSuffixTarget;
                        customLogger.Add(Constants.MessageAddedPrefabSuffix + rule.PrefabAddSuffixTarget);
                    }
                }

                // 'create a material for the mesh'
                // 'give the material this shader'
                if (!String.IsNullOrWhiteSpace(rule.MaterialShaderTargetRelativePath))
                {
                    var shader = AssetDatabase.LoadAssetAtPath<Shader>(rule.MaterialShaderTargetRelativePath);
                    if (shader == null)
                    {
                        continue;
                    }

                    var material = new Material(shader) {name = newModelName};
                    customLogger.Add(Constants.MessageCreatedMaterialForMesh);

                    // 'for naming, use <MeshName> + custom string'
                    if (rule.IsMaterialMeshNamePlusSuffix)
                    {
                        if (!String.IsNullOrWhiteSpace(rule.MaterialMeshNameSuffixTarget))
                        {
                            material.name = newModelName + rule.MaterialMeshNameSuffixTarget;
                        }
                    }
                    customLogger.Add(Constants.MessageMaterialName + material.name);
                    
                    // 'shader inputs and equivalent texture suffixes matchings'
                    foreach (var mapping in rule.MaterialShaderInputToTextureSuffixMapping)
                    {
                        // assign all textures that match the <MeshName>
                        var shaderProperty = mapping.Key;
                        var textureNameSuffix = mapping.Value;

                        var expectedTexturePath = Path.Combine(
                            assetDirectoryPath,
                            asset.name + textureNameSuffix + rule.MaterialTextureExtension);
                        
                        var texture = (Texture2D) AssetDatabase.LoadAssetAtPath(expectedTexturePath, typeof(Texture2D));
                        
                        // Exit early if the texture does not exist in the project
                        if (!texture)
                        {
                            // Debug.Log(ErrorTextureDoesNotExist + expectedTexturePath);
                            continue;
                        }
                        
                        // Assign texture to the appropriate material slot
                        material.SetTexture(shaderProperty, texture);
                        
                        customLogger.Add(Constants.MessageTextureMatch + expectedTexturePath);
                    }
                    
                    var renderer = modelInScene.GetComponent<Renderer>();
                    if (!renderer)
                    {
                        modelInScene.AddComponent<MeshRenderer>();
                        renderer = modelInScene.GetComponent<Renderer>();
                    }
                    
                    // Save the material to the same folder
                    AssetDatabase.CreateAsset(material, 
                        Path.Combine(assetDirectoryPath, material.name + Constants.MaterialExtension));

                    // Apply the material to the sub-mesh
                    renderer.material = material;
                    customLogger.Add(Constants.MessageAssignNewMaterialToMesh);

                    // Write all unsaved assets to disk
                    AssetDatabase.SaveAssets();
                }
                
                // Create a prefab
                PrefabUtility.SaveAsPrefabAssetAndConnect(
                    modelInScene,
                    Path.Combine(assetDirectoryPath, modelInScene.name + Constants.PrefabExtension),
                    InteractionMode.UserAction);
                customLogger.Add(Constants.MessageNewPrefabCreated);
                
                // Cleanup - remove asset from scene
                DestroyImmediate(modelInScene);
            }

            return customLogger;
        }
    }
}


