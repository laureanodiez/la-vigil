using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    public Camera fogCam;
    public RenderTexture fogRT;
    public Texture2D brush;        // tu Brush.png importado
    public float brushSize = 64f;  // píxeles

    void Update() {
        ClearFogAt(transform.position);
    }

    void ClearFogAt(Vector3 worldPos) {
        // 1) Convertir worldPos a coordenadas de textura
        Vector3 vp = fogCam.WorldToViewportPoint(worldPos);
        if (vp.x < 0||vp.x>1||vp.y<0||vp.y>1) return;
        int x = (int)(vp.x * fogRT.width) - (int)(brushSize/2);
        int y = (int)(vp.y * fogRT.height) - (int)(brushSize/2);

        // 2) Leer RenderTexture antigua a Texture2D (solo una vez al Start para rendimiento)
        RenderTexture.active = fogRT;
        Texture2D tmp = new Texture2D(fogRT.width, fogRT.height, TextureFormat.RGBA32, false);
        tmp.ReadPixels(new Rect(0,0,fogRT.width,fogRT.height), 0, 0);
        tmp.Apply();

        // 3) “Borrar” la niebla pintando el brush con Alpha = 0
        Color[] brushCols = brush.GetPixels();
        for (int i=0; i<brushSize; i++){
            for (int j=0; j<brushSize; j++){
                int px = x + i, py = y + j;
                if (px<0||px>=tmp.width||py<0||py>=tmp.height) continue;
                Color bc = brushCols[j*(int)brushSize + i];
                if (bc.a > 0.1f) tmp.SetPixel(px, py, new Color(0,0,0, 0));
            }
        }
        tmp.Apply();

        // 4) Escribir de nuevo al RenderTexture
        Graphics.Blit(tmp, fogRT);
        RenderTexture.active = null;

        Destroy(tmp);
    }
}
