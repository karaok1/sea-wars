using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class MinimapSetup
{
    [MenuItem("Tools/Setup Minimap Scene")]
    public static void SetupScene()
    {
        // 1. Create HUD Canvas
        GameObject canvasObj = GameObject.Find("HUD_Canvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("HUD_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 2. Create Minimap Container, Mask, Display, and Border
        GameObject containerObj = GameObject.Find("Minimap_Container");
        if (containerObj == null)
        {
            containerObj = new GameObject("Minimap_Container");
            containerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            
            // Layout (Top Right)
            containerRect.anchorMin = new Vector2(1, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.pivot = new Vector2(1, 1);
            containerRect.anchoredPosition = new Vector2(-20, -20);
            containerRect.sizeDelta = new Vector2(250, 250); // Slightly larger for border
        }

        // 2a. Create Mask (Circular)
        GameObject maskObj = GameObject.Find("Minimap_Mask");
        if (maskObj == null)
        {
            maskObj = new GameObject("Minimap_Mask");
            maskObj.transform.SetParent(containerObj.transform, false);
            
            Image maskImage = maskObj.AddComponent<Image>();
            maskImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            maskImage.type = Image.Type.Simple;
            
            maskObj.AddComponent<Mask>().showMaskGraphic = true;

            RectTransform maskRect = maskObj.GetComponent<RectTransform>();
            maskRect.anchorMin = new Vector2(0.5f, 0.5f);
            maskRect.anchorMax = new Vector2(0.5f, 0.5f);
            maskRect.pivot = new Vector2(0.5f, 0.5f);
            maskRect.anchoredPosition = Vector2.zero;
            maskRect.sizeDelta = new Vector2(200, 200); // Size of the visible minimap area
        }
        else
        {
             // Ensure it's a child of container if found but wrongly placed? 
             // Ideally we assume if found it's correct or we re-parent. 
             if (maskObj.transform.parent != containerObj.transform)
                 maskObj.transform.SetParent(containerObj.transform, false);
        }

        // 2b. Create Minimap Display (RawImage) - Child of Mask
        GameObject rawImageObj = GameObject.Find("Minimap_Display");
        if (rawImageObj == null)
        {
            rawImageObj = new GameObject("Minimap_Display");
            rawImageObj.transform.SetParent(maskObj.transform, false);
            RawImage rawImage = rawImageObj.AddComponent<RawImage>();
            
            RectTransform rect = rawImage.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero; // Full stretch
            
            // Assign Texture
            string path = "Assets/Textures/MinimapRT.renderTexture";
            RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
            if (rt != null) rawImage.texture = rt;
        }
        else
        {
             if (rawImageObj.transform.parent != maskObj.transform) 
                 rawImageObj.transform.SetParent(maskObj.transform, false);
             
             // Ensure it stretches
             RectTransform rect = rawImageObj.GetComponent<RectTransform>();
             rect.anchorMin = new Vector2(0, 0);
             rect.anchorMax = new Vector2(1, 1);
             rect.offsetMin = Vector2.zero;
             rect.offsetMax = Vector2.zero;
        }

        // 2c. Create Minimap Border - Child of Container (Sibling of Mask, draws on top)
        GameObject borderObj = GameObject.Find("Minimap_Border");
        if (borderObj == null)
        {
            borderObj = new GameObject("Minimap_Border");
            borderObj.transform.SetParent(containerObj.transform, false);
            Image borderImage = borderObj.AddComponent<Image>();
            
            // Load Border Sprite
            string borderPath = "Assets/Images/minimap_hud.png";
            Sprite borderSprite = AssetDatabase.LoadAssetAtPath<Sprite>(borderPath);
            if (borderSprite != null) 
            {
                borderImage.sprite = borderSprite;
            }
            else
            {
                Debug.LogError($"Border sprite not found at {borderPath}");
            }

            RectTransform borderRect = borderObj.GetComponent<RectTransform>();
            borderRect.anchorMin = new Vector2(0.5f, 0.5f);
            borderRect.anchorMax = new Vector2(0.5f, 0.5f);
            borderRect.pivot = new Vector2(0.5f, 0.5f);
            borderRect.anchoredPosition = Vector2.zero;
            borderRect.sizeDelta = new Vector2(250, 250); // Match container/border size
        }
        else
        {
             if (borderObj.transform.parent != containerObj.transform)
                 borderObj.transform.SetParent(containerObj.transform, false);
        }

        // 3. Create or Find Minimap Camera (2D: positioned on Z axis, looking forward)
        GameObject cameraObj = GameObject.Find("Minimap_Camera");
        Camera cam;
        if (cameraObj == null)
        {
            cameraObj = new GameObject("Minimap_Camera");
            cam = cameraObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 10;
            // 2D game: position on Z axis, looking forward (no rotation)
            cam.transform.position = new Vector3(0, 0, -10);
            cam.transform.rotation = Quaternion.identity;
        }
        else
        {
            cam = cameraObj.GetComponent<Camera>();
        }
        
        // Always assign Target Texture
        string rtPath = "Assets/Textures/MinimapRT.renderTexture";
        RenderTexture renderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
        if (renderTexture != null) 
        {
            cam.targetTexture = renderTexture;
            Debug.Log($"Assigned RenderTexture to Minimap_Camera: {rtPath}");
        }
        else
        {
            Debug.LogError($"RenderTexture not found at {rtPath}");
        }

        // URP Fix: Add UniversalAdditionalCameraData if missing
        var cameraData = cameraObj.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null)
        {
            cameraData = cameraObj.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderShadows = false; 
            cameraData.renderType = CameraRenderType.Base; 
        }

        // 4. Setup Minimap Controller
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Attach controller
            MinimapController controller = cameraObj.GetComponent<MinimapController>();
            if (controller == null) controller = cameraObj.AddComponent<MinimapController>();
            
            controller.player = player.transform;
        }

        Debug.Log("Minimap Scene Setup Complete (URP Updated)!");
    }
}