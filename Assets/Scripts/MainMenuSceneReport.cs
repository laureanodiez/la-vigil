using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
#if UNITY_2021_2_OR_NEWER
using UnityEngine.Rendering;
#endif

public class MainMenuSceneReport : MonoBehaviour
{
    // Se ejecuta justo después de que la escena haya cargado en runtime
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void GenerateReport()
    {
        Debug.Log("=== MainMenuSceneReport START ===");

        try
        {
            // 1) Pipeline / Graphics info
            var rp = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
            Debug.Log("[Graphics] RenderPipelineAsset: " + (rp ? rp.name : "null / Built-in pipeline"));

            // 2) Cameras
            Camera[] cams = Object.FindObjectsOfType<Camera>(true);
            Debug.Log($"[Cameras] Count = {cams.Length}");
            for (int i = 0; i < cams.Length; i++)
            {
                Camera c = cams[i];
                Debug.Log($"Camera[{i}] name='{c.name}' enabled={c.enabled} HDR={c.allowHDR} orthographic={c.orthographic} targetTexture={(c.targetTexture!=null ? c.targetTexture.name : "null")}, clearFlags={c.clearFlags}");
            }

            // 3) UI Images with custom material
            var images = Object.FindObjectsOfType<UnityEngine.UI.Image>(true);
            Debug.Log($"[UI Images] Count = {images.Length}");
            int uiCount = 0;
            for (int i=0;i<images.Length;i++){
                var im = images[i];
                if (im.material != null)
                {
                    Debug.Log($"UI Image[{i}] GO='{im.gameObject.name}' material='{im.material.name}' shader='{(im.material.shader!=null?im.material.shader.name:"null")}'");
                    uiCount++;
                }
            }
            if (uiCount==0) Debug.Log("[UI Images] No UI images with custom material found.");

            // 4) SpriteRenderers
            var srs = Object.FindObjectsOfType<SpriteRenderer>(true);
            Debug.Log($"[SpriteRenderers] Count = {srs.Length}");
            var shaderSet = new HashSet<string>();
            for (int i=0;i<srs.Length;i++){
                var sr = srs[i];
                var m = sr.sharedMaterial;
                string sname = m && m.shader ? m.shader.name : "no-material";
                Debug.Log($"SpriteRenderer[{i}] GO='{sr.gameObject.name}' material='{(m?m.name:"null")}' shader='{sname}'");
                if (m && m.shader) shaderSet.Add(sname);
            }

            // 5) MeshRenderers and ParticleSystemRenderers
            var mrs = Object.FindObjectsOfType<MeshRenderer>(true);
            Debug.Log($"[MeshRenderers] Count = {mrs.Length}");
            for (int i=0;i<mrs.Length;i++){
                var mr = mrs[i];
                var mats = mr.sharedMaterials;
                for (int j=0;j<mats.Length;j++){
                    var mat = mats[j];
                    string sname = mat && mat.shader ? mat.shader.name : "no-material";
                    Debug.Log($"MeshRenderer[{i}] GO='{mr.gameObject.name}' material[{j}]='{(mat?mat.name:"null")}' shader='{sname}'");
                    if (mat && mat.shader) shaderSet.Add(sname);
                }
            }

            var prrs = Object.FindObjectsOfType<ParticleSystemRenderer>(true);
            Debug.Log($"[ParticleSystemRenderer] Count = {prrs.Length}");
            for (int i=0;i<prrs.Length;i++){
                var pr = prrs[i];
                var mat = pr.sharedMaterial;
                string sname = mat && mat.shader ? mat.shader.name : "no-material";
                Debug.Log($"ParticleRenderer[{i}] GO='{pr.gameObject.name}' material='{(mat?mat.name:"null")}' shader='{sname}'");
                if (mat && mat.shader) shaderSet.Add(sname);
            }

            // 6) List unique shaders found
            Debug.Log("[Shaders found in scene materials] Count = " + shaderSet.Count);
            foreach (var s in shaderSet)
            {
                Debug.Log("   shader -> " + s);
            }

            // 7) Volumes (post-processing)
            var volumes = Object.FindObjectsOfType<Volume>(true);
            Debug.Log($"[Volumes] Count = {volumes.Length}");
            for (int i=0;i<volumes.Length;i++){
                Debug.Log($"Volume[{i}] GO='{volumes[i].gameObject.name}' profile='{(volumes[i].profile?volumes[i].profile.name:"null")}' isGlobal={volumes[i].isGlobal}");
            }

            // 8) Any RenderTextures referenced by Cameras? (we try to reflect on UniversalAdditionalCameraData if present)
            var cameras = Object.FindObjectsOfType<Camera>(true);
            for (int i=0;i<cameras.Length;i++){
                var cam = cameras[i];
                // try targetTexture first
                if (cam.targetTexture != null)
                    Debug.Log($"Camera '{cam.name}' has targetTexture '{cam.targetTexture.name}'");
                // try to find any component that could hold output texture using reflection as a fallback
                var comp = cam.GetComponent("UniversalAdditionalCameraData");
                if (comp != null)
                {
                    Debug.Log($"Camera '{cam.name}' has UniversalAdditionalCameraData component (exists).");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("MainMenuSceneReport ERROR: " + ex.ToString());
        }
        finally
        {
            Debug.Log("=== MainMenuSceneReport END ===");
        }
    }
}
