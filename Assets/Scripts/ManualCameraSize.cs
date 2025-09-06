using UnityEngine;
using Unity.Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[AddComponentMenu("Cinemachine/Manual Camera Fit")]
public class ManualCameraFit : MonoBehaviour
{
    [Header("Asignar la CinemachineCamera")]
    public CinemachineCamera vcam;

    [Header("Dimensiones deseadas (en unidades de mundo)")]
    public float manualWidth = 16f;
    public float manualHeight = 9f;

    [Header("Preview")]
    public bool autoApplyInEditor = false; // si querés que se aplique automáticamente al mover sliders

    public void ApplySize()
    {
        if (vcam == null) return;

        float aspect = (float)Screen.width / Screen.height;

        float sizeFromHeight = manualHeight / 2f;
        float sizeFromWidth = manualWidth / (2f * aspect);

        float targetOrtho = Mathf.Max(sizeFromHeight, sizeFromWidth);

        var lens = vcam.Lens;
        lens.OrthographicSize = targetOrtho;
        vcam.Lens = lens;

        Debug.Log($"[ManualCameraFit] Applied size {targetOrtho:F3} (W={manualWidth}, H={manualHeight}, aspect={aspect:F2}) on {vcam.name}");
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (autoApplyInEditor && vcam != null)
            ApplySize();
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ManualCameraFit))]
public class ManualCameraFitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var script = (ManualCameraFit)target;
        GUILayout.Space(6);

        if (GUILayout.Button("Apply Size Now"))
        {
            script.ApplySize();
            EditorUtility.SetDirty(script.vcam);
        }

        float aspect = (float)Screen.width / Screen.height;
        float predicted = Mathf.Max(script.manualHeight / 2f, script.manualWidth / (2f * aspect));
        EditorGUILayout.LabelField("Predicted OrthoSize:", predicted.ToString("F3"));
    }
}
#endif
