using UnityEngine;

public class DoorSceneTrigger : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena a cargar")]
    public string sceneToLoad = "Presente";

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // comprueba tag Player; si no lo usás podés buscar por componente
        if (other.CompareTag("Player"))
        {
            // Llama al controller singleton que maneja el fade + carga
            SceneTransitionController.Instance.TransitionToScene(sceneToLoad);
        }
    }
}
