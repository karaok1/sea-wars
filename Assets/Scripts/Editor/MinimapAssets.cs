using UnityEngine;
using UnityEditor;

public class MinimapAssets
{
    [MenuItem("Tools/Generate Minimap Texture")]
    public static void GenerateMinimapTexture()
    {
        string path = "Assets/Textures/MinimapRT.renderTexture";
        
        if (AssetDatabase.LoadAssetAtPath<RenderTexture>(path) != null)
        {
            Debug.LogWarning($"RenderTexture already exists at {path}");
            return;
        }

        RenderTexture rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
        rt.name = "MinimapRT";
        
        AssetDatabase.CreateAsset(rt, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Created Minimap RenderTexture at {path}");
    }
}