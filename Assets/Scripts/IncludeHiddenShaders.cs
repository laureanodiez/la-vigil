using UnityEngine;

// Ejecutar muy pronto en el startup para evitar strip
[DefaultExecutionOrder(-1000)]
public class IncludeHiddenShaders : MonoBehaviour
{
    void Awake()
    {
        // Nombres tal como aparecen en tu consola
        Shader.Find("Hidden/CoreSRP/CoreCopy");
        Shader.Find("Hidden/Universal/HDRDebugView");

        // Variantes comunes útiles a forzar (añade si los ves en consola)
        Shader.Find("Hidden/Universal/BlitCopy");
        Shader.Find("Hidden/Universal/Blit");
        Shader.Find("Hidden/Universal/BlitCopyPass");

        // Shaders URP 2D (si usás 2D lit/unlit)
        Shader.Find("Universal Render Pipeline/2D/Sprite-Lit");
        Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit");
        Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
    }
}
