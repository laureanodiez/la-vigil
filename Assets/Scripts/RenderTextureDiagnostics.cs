using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class RenderTextureDiagnosticsPersistent : MonoBehaviour
{
    // Crear automáticamente el objeto al inicio y hacerlo persistent
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CreatePersistentInstance()
    {
        var go = new GameObject("RTDiagnostics");
        DontDestroyOnLoad(go);
        go.AddComponent<RenderTextureDiagnosticsPersistent>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Se ejecuta cada vez que se carga una escena (incluye Pasado)
        RunDiagnostics();
    }

    // Método público que también podés invocar desde DevTools (SendMessage)
    public void RunDiagnostics()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== RenderTexture Diagnostics (scene='{SceneManager.GetActiveScene().name}') START ===");

        Camera[] cams = Object.FindObjectsOfType<Camera>(true);
        sb.AppendLine($"Cameras found: {cams.Length}");
        foreach (var c in cams)
        {
            sb.AppendLine($" Camera '{c.name}' enabled={c.enabled} ortho={c.orthographic} HDR={c.allowHDR} targetTexture={(c.targetTexture!=null?c.targetTexture.name:"null")}");
        }

        // RawImages that point to a RenderTexture
        var rawImages = Object.FindObjectsOfType<RawImage>(true);
        sb.AppendLine($"RawImages found: {rawImages.Length}");
        HashSet<RenderTexture> referencedRTs = new HashSet<RenderTexture>();
        for (int i = 0; i < rawImages.Length; i++)
        {
            var ri = rawImages[i];
            if (ri.texture is RenderTexture rt)
            {
                sb.AppendLine($"  RawImage '{ri.gameObject.name}' uses RenderTexture '{rt.name}'");
                referencedRTs.Add(rt);
            }
        }

        // Scan materials' mainTexture (runtime-safe)
        HashSet<Material> mats = new HashSet<Material>();
        foreach (var sr in Object.FindObjectsOfType<SpriteRenderer>(true))
            if (sr.sharedMaterial != null) mats.Add(sr.sharedMaterial);
        foreach (var mr in Object.FindObjectsOfType<MeshRenderer>(true))
            foreach (var m in mr.sharedMaterials) if (m!=null) mats.Add(m);
        foreach (var pr in Object.FindObjectsOfType<ParticleSystemRenderer>(true))
            if (pr.sharedMaterial != null) mats.Add(pr.sharedMaterial);
        foreach (var ui in Object.FindObjectsOfType<UnityEngine.UI.Image>(true))
            if (ui.material != null) mats.Add(ui.material);

        sb.AppendLine($"Materials scanned: {mats.Count}");
        foreach (var m in mats)
        {
            if (m == null) continue;
            var t = m.mainTexture;
            if (t is RenderTexture rt2)
            {
                sb.AppendLine($"  Material '{m.name}' mainTexture references RenderTexture '{rt2.name}'");
                referencedRTs.Add(rt2);
            }
        }

        // Cross-check cameras that write to RTs referenced by materials/UI
        foreach (var c in cams)
        {
            if (c == null) continue;
            if (c.targetTexture != null && referencedRTs.Contains(c.targetTexture))
            {
                sb.AppendLine($"*** FEEDBACK LOOP DETECTED: Camera '{c.name}' writes to RT '{c.targetTexture.name}' AND THAT RT IS SAMPLED BY MATERIAL/UI.");
            }
            // also try checking UniversalAdditionalCameraData outputTexture via reflection if present
            var uacd = c.GetComponent("UniversalAdditionalCameraData");
            if (uacd != null)
            {
                var prop = uacd.GetType().GetProperty("outputTexture");
                if (prop != null)
                {
                    var obj = prop.GetValue(uacd, null) as RenderTexture;
                    if (obj != null && referencedRTs.Contains(obj))
                    {
                        sb.AppendLine($"*** FEEDBACK LOOP DETECTED (UACD): Camera '{c.name}' UACD.outputTexture '{obj.name}' is sampled by material/UI.");
                    }
                }
            }
        }

        if (referencedRTs.Count == 0) sb.AppendLine("No RenderTextures referenced by materials/UI in this scene.");

        sb.AppendLine("=== RenderTexture Diagnostics END ===");
        Debug.Log(sb.ToString());
    }
}
