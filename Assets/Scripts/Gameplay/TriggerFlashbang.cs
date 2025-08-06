using UnityEngine;

public class TriggerFlashbang : MonoBehaviour
{
    [SerializeField] private PlayerFlashbangSystem playerSystem;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            playerSystem.StartFlashbangSequence();
            GetComponent<Collider2D>().enabled = false; //Desactiva el trigger
        }
    }
}
