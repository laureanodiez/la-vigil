using UnityEngine;

public class DetectorDeDesactivacion : MonoBehaviour
{
    void OnDisable()
    {
        // Esto imprime "quién" (la pila de llamadas) ordenó que me apagara
        Debug.LogWarning($"¡ME APAGARON! El objeto '{name}' fue desactivado.");
        Debug.Log("Rastro del culpable:\n" + System.Environment.StackTrace);
    }
}