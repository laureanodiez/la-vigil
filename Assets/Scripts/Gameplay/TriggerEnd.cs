using UnityEngine;

public class TriggerEnd : MonoBehaviour
{
    [SerializeField] private PlayerFlashbangSystem playerSystem;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            playerSystem.GoodEndFlashSequence();
            GetComponent<Collider2D>().enabled = false; //Desactiva el trigger
        }
    }
}
