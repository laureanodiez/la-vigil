using System;
using UnityEngine;

[DefaultExecutionOrder(-9999)]
public class CoroutineExceptionCatcher : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        var go = new GameObject("CoroutineExceptionCatcher");
        DontDestroyOnLoad(go);
        go.AddComponent<CoroutineExceptionCatcher>();
    }

    void OnEnable()
    {
        Application.logMessageReceived += OnLog;
    }
    void OnDisable()
    {
        Application.logMessageReceived -= OnLog;
    }

    void OnLog(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            // Print the full managed stack trace and a headphone marker for easy finding
            Debug.Log("[EXCEPTION CAPTURED] " + condition + "\n" + stackTrace);
        }
    }
}
