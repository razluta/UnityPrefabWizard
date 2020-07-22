
using UnityEngine;

namespace UnityPrefabWizard.AssetUtilities
{
    public static class GeneralUtilities
    {
        public static Color GetRandomColor()
        {
            return new Color(
                Random.Range(0.0f, 1.0f),
                Random.Range(0.0f, 1.0f),
                Random.Range(0.0f, 1.0f));
        }
    }
}