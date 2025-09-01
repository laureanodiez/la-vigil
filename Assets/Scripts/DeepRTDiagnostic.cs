using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Reflection;

public class DeepRTDiagnosticSafe : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateInstance()
    {
        var go = new GameObject("DeepRTDiagnosticSafe");
        DontDestroyOnLoad(go);
        go.AddComponent<DeepRTDiagnosticSafe>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        RunDeepDiagnostic();
    }

    public void RunDeepDiagnostic()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== DeepRTDiagnosticSafe (scene='{SceneManager.GetActiveScene().name}') START ===");

        // 1) Cameras and their targetTexture
        var cams = Object.FindObjectsOfType<Camera>(true);
        sb.AppendLine($"Cameras found: {cams.Length}");
        foreach (var c in cams)
        {
            sb.AppendLine($" Camera '{c.name}' enabled={c.enabled} ortho={c.orthographic} HDR={c.allowHDR} targetTexture={(c.targetTexture!=null?c.targetTexture.name:"null")}");
            // Try UACD.outputTexture via reflection (if present)
            var uacd = c.GetComponent("UniversalAdditionalCameraData");
            if (uacd != null)
            {
                var prop = uacd.GetType().GetProperty("outputTexture");
                if (prop != null)
                {
                    var val = prop.GetValue(uacd, null) as RenderTexture;
                    sb.AppendLine($"  -> UACD.outputTexture = {(val!=null?val.name:"null")}");
                }
            }
        }

        // container for found RTs
        HashSet<RenderTexture> foundRTs = new HashSet<RenderTexture>();

        // 2) RawImages that use RenderTexture
        var rawImages = Object.FindObjectsOfType<UnityEngine.UI.RawImage>(true);
        sb.AppendLine($"RawImages found: {rawImages.Length}");
        foreach (var ri in rawImages)
        {
            if (ri == null) continue;
            sb.AppendLine($" RawImage '{ri.gameObject.name}' uses texture '{(ri.texture!=null?ri.texture.name:"null")}' ({ri.texture?.GetType().Name})");
            if (ri.texture is RenderTexture rt) foundRTs.Add(rt);
        }

        // 3) Collect materials in scene and loaded assets
        HashSet<Material> mats = new HashSet<Material>();
        foreach (var sr in Object.FindObjectsOfType<SpriteRenderer>(true)) if (sr.sharedMaterial!=null) mats.Add(sr.sharedMaterial);
        foreach (var mr in Object.FindObjectsOfType<MeshRenderer>(true))
            foreach (var mm in mr.sharedMaterials) if (mm!=null) mats.Add(mm);
        foreach (var pr in Object.FindObjectsOfType<ParticleSystemRenderer>(true)) if (pr.sharedMaterial!=null) mats.Add(pr.sharedMaterial);
        foreach (var ui in Object.FindObjectsOfType<UnityEngine.UI.Image>(true)) if (ui.material!=null) mats.Add(ui.material);

        // Also include materials loaded in memory (assets)
        foreach (var am in Resources.FindObjectsOfTypeAll<Material>()) if (am!=null) mats.Add(am);

        sb.AppendLine($"Materials scanned: {mats.Count}");

        // common texture property names to check (covers most shaders)
        string[] commonTexProps = new string[] {
            "_MainTex", "_BaseMap", "_BaseColorMap", "_BaseColor", "_BaseMap", "_BumpMap", "_NormalMap",
            "_EmissionMap", "_MetallicGlossMap", "_MetallicMap", "_OcclusionMap", "_DetailAlbedoMap",
            "_DetailNormalMap", "_MaskMap", "_CameraOpaqueTexture", "_CameraColorTexture", "_GrabTexture",
            "_CameraDepthTexture", "_CameraDepthTextureRT", "_MainTex_ST"
        };

        foreach (var m in mats)
        {
            if (m == null) continue;
            Shader sh = m.shader;
            sb.AppendLine($" Material '{m.name}' shader='{(sh!=null?sh.name:"null")}'");
            // check mainTexture quickly
            try {
                if (m.mainTexture != null)
                {
                    sb.AppendLine($"   -> mainTexture = '{m.mainTexture.name}' ({m.mainTexture.GetType().Name})");
                    if (m.mainTexture is RenderTexture rtx) foundRTs.Add(rtx);
                }
            } catch {}

            // check common properties
            foreach (var pname in commonTexProps)
            {
                try
                {
                    var tex = m.GetTexture(pname);
                    if (tex != null)
                    {
                        sb.AppendLine($"   -> Prop '{pname}' = '{tex.name}' ({tex.GetType().Name})");
                        if (tex is RenderTexture rt2) foundRTs.Add(rt2);
                    }
                }
                catch { }
            }
        }

        // 4) Check common global shader textures
        string[] globals = new string[] {
            "_MainTex","_CameraOpaqueTexture","_CameraColorTexture","_GrabTexture","_GrabPassTexture",
            "_CameraDepthTexture","_GlobalTexture"
        };
        sb.AppendLine("Checking common global shader textures:");
        foreach (var g in globals)
        {
            try {
                var gt = Shader.GetGlobalTexture(g);
                if (gt!=null)
                {
                    sb.AppendLine($"  Global '{g}' -> '{gt.name}' ({gt.GetType().Name})");
                    if (gt is RenderTexture r) foundRTs.Add(r);
                }
            } catch {}
        }

        // 5) Report found RTs
        sb.AppendLine($"RenderTextures discovered: {foundRTs.Count}");
        foreach (var r in foundRTs) sb.AppendLine($"  RT -> {r.name} desc: width={r.width} height={r.height} format={r.format}");

        // 6) Cross-check cameras writing to those RTs
        foreach (var c in cams)
        {
            if (c==null) continue;
            if (c.targetTexture!=null && foundRTs.Contains(c.targetTexture))
                sb.AppendLine($"*** FEEDBACK RISK: Camera '{c.name}' writes to RT '{c.targetTexture.name}' which is referenced by materials/UI/global.");
            var uacd = c.GetComponent("UniversalAdditionalCameraData");
            if (uacd != null)
            {
                var prop = uacd.GetType().GetProperty("outputTexture");
                if (prop!=null)
                {
                    var val = prop.GetValue(uacd, null) as RenderTexture;
                    if (val!=null && foundRTs.Contains(val))
                        sb.AppendLine($"*** FEEDBACK RISK (UACD): Camera '{c.name}' UACD.outputTexture '{val.name}' is referenced by materials/UI/global.");
                }
            }
        }

        if (foundRTs.Count==0) sb.AppendLine("No RenderTextures referenced by materials/UI/global detected in this scan.");

        sb.AppendLine("=== DeepRTDiagnosticSafe END ===");
        Debug.Log(sb.ToString());
    }
}
