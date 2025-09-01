// Assets/Scripts/BuildSceneInspector.cs
using UnityEngine;
using System.Text;
using UnityEngine.UI;

public class BuildSceneInspector : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Run()
    {
        var go = new GameObject("BuildSceneInspector");
        DontDestroyOnLoad(go);
        go.AddComponent<BuildSceneInspector>();
    }

    void Start()
    {
        DumpAll();
    }

    void DumpAll()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== BuildSceneInspector START ===");
        sb.AppendLine("Active Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        // System / Graphics info
        sb.AppendLine("System Graphics: " + SystemInfo.graphicsDeviceVendor + " / " + SystemInfo.graphicsDeviceName + " / " + SystemInfo.graphicsDeviceType);
        sb.AppendLine("WebGL context? " + (Application.platform == RuntimePlatform.WebGLPlayer));

        // Cameras
        var cams = Object.FindObjectsOfType<Camera>(true);
        sb.AppendLine("Cameras found: " + cams.Length);
        foreach (var c in cams)
        {
            sb.AppendLine($" Camera '{c.name}' enabled={c.enabled} ortho={c.orthographic} HDR={c.allowHDR} clearFlags={c.clearFlags} cullingMask=0x{((int)c.cullingMask):X} targetTexture='{(c.targetTexture!=null?c.targetTexture.name:"null")}' depth={c.depth}");
            // UACD reflection: try get outputTexture if present
            var uacd = c.GetComponent("UniversalAdditionalCameraData");
            if (uacd != null)
            {
                var prop = uacd.GetType().GetProperty("outputTexture");
                if (prop != null)
                {
                    var val = prop.GetValue(uacd, null) as RenderTexture;
                    sb.AppendLine($"   -> UACD.outputTexture = {(val!=null?val.name:"null")}");
                }
            }
        }

        // RawImages using RenderTextures
        var raws = Object.FindObjectsOfType<RawImage>(true);
        sb.AppendLine("RawImages found: " + raws.Length);
        foreach (var r in raws) sb.AppendLine($" RawImage '{r.name}' texture='{(r.texture!=null?r.texture.name:"null")}'");

        // SpriteRenderers - but focus: background sprites (by name "Background" or tag "Background") and Player
        var srs = Object.FindObjectsOfType<SpriteRenderer>(true);
        sb.AppendLine("SpriteRenderers total: " + srs.Length);
        for (int i=0;i<srs.Length;i++)
        {
            var sr = srs[i];
            string matName = sr.sharedMaterial ? sr.sharedMaterial.name : "null";
            string shaderName = (sr.sharedMaterial && sr.sharedMaterial.shader) ? sr.sharedMaterial.shader.name : "(uses default Sprite shader or null)";
            sb.AppendLine($" #{i} GO='{sr.gameObject.name}' layer={sr.gameObject.layer} tag={sr.gameObject.tag} sprite='{(sr.sprite?sr.sprite.name:"null")}' enabled={sr.enabled} sortingLayer='{sr.sortingLayerName}' order={sr.sortingOrder} material='{matName}' shader='{shaderName}' spriteTex='{(sr.sprite && sr.sprite.texture? sr.sprite.texture.name : "null")}'");
        }

        // Check for RenderTextures in materials/globals
        sb.AppendLine("--- Checking materials for RenderTexture references ---");
        var mats = Resources.FindObjectsOfTypeAll<Material>();
        int rtcount = 0;
        foreach (var m in mats)
        {
            if (m == null) continue;
            // search common props
            string[] props = new string[] {"_MainTex","_BaseMap","_CameraOpaqueTexture","_GrabTexture","_CameraColorTexture","_CameraDepthTexture","_MaskTex"};
            foreach (var p in props)
            {
                try {
                    var tex = m.GetTexture(p);
                    if (tex is RenderTexture)
                    {
                        sb.AppendLine($" Material '{m.name}' shader='{m.shader?.name}' prop='{p}' -> RT '{tex.name}'");
                        rtcount++;
                    }
                } catch {}
            }
        }
        sb.AppendLine("RenderTextures found referenced in materials: " + rtcount);

        sb.AppendLine("=== BuildSceneInspector END ===");
        Debug.Log(sb.ToString());
    }
}
