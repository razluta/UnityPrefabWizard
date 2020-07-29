namespace UnityPrefabWizard
{
    public static class Constants
    {
        public const string VersionNumber = "v.0.1.0.20200727";

        public const int WindowWidth = 500;
        public const int WindowHeight = 800;
        public const string WindowName = "Prefab Wizard";
        public const string WindowMenuPath = "Art Tools/" + WindowName;
        
        public const string PrefabExtension = ".prefab";
        public const string JsonExtension = "json";
        public const string MaterialExtension = ".mat";
        public const string DefaultRulesFileName = "Rules";

        public const string LabelUxmlMainPrefabWizard = "CS_PrefabWizard";
        public const string LabelListViewRulesList = "LV_RulesList";
        public const string LabelUxmlSingleRule = "CS_SingleRule";
        public const string LabelVisualElementSingleRule = "VE_SingleRule";
        public const string LabelButtonLoadRules = "BT_LoadRules";
        public const string LabelButtonSaveRules = "BT_SaveRules";
        public const string LabelButtonAddRule = "BT_AddRule";
        public const string LabelButtonClearRules = "BT_ClearRules";
        public const string LabelButtonCreatePrefab = "BT_CreatePrefab";
        public const string LabelListViewLog = "LV_Log";
        public const string LabelUxmlLogEntry = "CS_LogEntry";
        public const string LabelButtonLogEntry = "BT_LogEntry";
        public const string LabelButtonClearLog = "BT_ClearLog";
        public const string LabelLabelVersion = "LB_Version";

        public const string LabelButtonNameStartsWith = "BT_IncludeNameStartsWith";
        public const string LabelFoldoutIncludeNameStartsWith = "FO_IncludeNameStartsWith";
        public const string LabelButtonNameContains = "BT_IncludeNameContains";
        public const string LabelFoldoutIncludeNameContains = "FO_IncludeNameContains";
        public const string LabelToggleUseMeshName = "TG_UseMeshName";
        public const string LabelToggleUseMeshNameReplace = "TG_UseMeshNameReplace";
        public const string LabelTextFieldUseMeshNameReplaceSource = "TF_UseMeshNameReplaceSource";
        public const string LabelTextFieldUseMeshNameReplaceTarget = "TF_UseMeshNameReplaceTarget";
        public const string LabelToggleUseUniqueName = "TG_UseUniqueName";
        public const string LabelTextFieldUseUniqueNameTarget = "TF_UseUniqueNameTarget";
        public const string LabelToggleAddSuffix = "TG_AddSuffix";
        public const string LabelTextFieldAddSuffixTarget = "TF_AddSuffixTarget";
        public const string LabelToggleCreateMaterialForMesh = "TG_CreateMaterialForMesh";
        public const string LabelObjectFieldShader = "OF_Shader";
        public const string LabelToggleMaterialUseMeshNamePlusSuffix = "TG_MaterialUseMeshName";
        public const string LabelTextFieldMaterialUseMeshNamePlusSuffixTarget = "TF_MaterialUseMeshNameTarget";
        public const string LabelFoldoutTextureInputs = "FO_TextureInputs";
        public const string LabelTextFieldTextureExtensionTarget = "TF_TextureExtensionTarget";
        public const string LabelToggleAssignAllTexturesToMaterial = "TG_AssignAllTexturesToMaterial";
        public const string LabelToggleAssignMaterialToMesh = "TG_AssignMaterialToMesh";

        public const string LabelButtonAddTextureInputMatching = "BT_AddTextureInputMatching";
        public const string LabelButtonRemove = "BT_Remove";
        
        public const string TitleError = "Error";
        public const string MessageErrorBodySelectOneMesh = "Please select one mesh in the project window!";
        public const string MessageErrorTextureDoesNotExist = "The expected texture does not exist in the project: ";
        public const string LabelButtonErrorOk = "OK";
        public const string MessageSuccessfullySavedRules = "Successfully saved rules to path: ";
        public const string MessageSuccessfullyLoadedRules = "Successfully loaded rules from path: ";
    }
}

