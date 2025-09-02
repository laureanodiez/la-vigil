using System;
using System.Text;
using UnityEngine;
using System.Reflection;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class SceneReferenceChecker : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureExists()
    {
        var go = new GameObject("SceneReferenceChecker");
        DontDestroyOnLoad(go);
        go.AddComponent<SceneReferenceChecker>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        CheckScene(s.name);
    }

    void Start()
    {
        CheckScene(SceneManager.GetActiveScene().name);
    }

    void CheckScene(string sceneName)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== SceneReferenceChecker: scene='{sceneName}' START ===");

        // List singletons / static holders that commonly keep scene refs:
        // this is heuristic: search for types that have static fields that are UnityEngine.Objects
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch { continue; }
            foreach (var t in types)
            {
                FieldInfo[] sfields;
                try { sfields = t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic); } catch { continue; }
                foreach (var f in sfields)
                {
                    if (!typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType)) continue;
                    object val = null;
                    try { val = f.GetValue(null); } catch { continue; }
                    if (val == null) continue;
                    // if the static field contains a scene object, log it
                    var uobj = val as UnityEngine.Object;
                    if (uobj != null && !string.IsNullOrEmpty(uobj.name))
                    {
                        // check if it's scene-bound (not an asset)
                        if (uobj is GameObject go)
                        {
                            if (!IsPersistent(go)) sb.AppendLine($"Static field {t.FullName}.{f.Name} holds Scene GO: '{go.name}'");
                        }
                        else
                        {
                            // components or assets
                            var path = GetObjectScenePath(uobj);
                            if (!string.IsNullOrEmpty(path))
                                sb.AppendLine($"Static field {t.FullName}.{f.Name} holds scene object '{uobj.name}' -> {path}");
                        }
                    }
                }
            }
        }

        // Scan all MonoBehaviours in scene and reflect serialized fields
        var mbs = FindObjectsOfType<MonoBehaviour>(true);
        sb.AppendLine($"MonoBehaviours found: {mbs.Length}");
        int issues = 0;
        foreach (var mb in mbs)
        {
            if (mb == null) continue;
            var mt = mb.GetType();
            var fields = mt.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var fi in fields)
            {
                // only check reference types and UnityEngine.Object
                if (fi.FieldType.IsValueType) continue;
                if (fi.GetCustomAttribute<NonSerializedAttribute>() != null) continue;
                // treat [SerializeField] and public fields
                bool isSerialized = fi.IsPublic || fi.GetCustomAttribute<SerializeField>() != null;
                if (!isSerialized) continue;
                object val = null;
                try { val = fi.GetValue(mb); } catch { continue; }
                if (val == null)
                {
                    issues++;
                    sb.AppendLine($" NULL REF: GameObject='{mb.gameObject.name}' Component='{mt.Name}' field='{fi.Name}' ({fi.FieldType.Name})");
                }
            }
        }

        sb.AppendLine($"Total null serialized references found: {issues}");
        sb.AppendLine("=== SceneReferenceChecker END ===");
        Debug.Log(sb.ToString());
    }

    static bool IsPersistent(GameObject go)
    {
        // heuristic: object is in DontDestroyOnLoad scene?
        return go.scene.name == "DontDestroyOnLoad";
    }

    static string GetObjectScenePath(UnityEngine.Object o)
    {
        if (o == null) return null;
        var go = o as GameObject;
        if (go != null) return GetHierarchyPath(go.transform);
        var comp = o as Component;
        if (comp != null) return GetHierarchyPath(comp.transform);
        return null;
    }
    static string GetHierarchyPath(Transform t)
    {
        string p = t.name;
        while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }
}
