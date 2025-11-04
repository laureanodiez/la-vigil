using UnityEngine;

public class TriggerEscombros : MonoBehaviour
{
    [SerializeField] private GameObject escombros;
    [SerializeField] private CinematicEffects cinematicManager;
    private bool yaActivado = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !yaActivado)
        {
            yaActivado = true;
            
            // Detener jugador
            var player = other.gameObject;
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            
            var movimiento = player.GetComponent<MonoBehaviour>();
            if (movimiento != null)
            {
                movimiento.enabled = false;
                StartCoroutine(ReactivarMovimiento(movimiento, 1f));
            }

            // Activar secuencia
            cinematicManager.PlaySequence("CaidaEscombros");
        }
    }

    System.Collections.IEnumerator ReactivarMovimiento(MonoBehaviour script, float delay)
    {
        yield return new WaitForSeconds(delay);
        script.enabled = true;
    }
}