using UnityEngine;

public class TriggerEnd : MonoBehaviour
{
    [SerializeField] private PlayerFlashbangSystem playerSystem;
    [SerializeField] private GameObject escombros;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerSystem.GoodEndFlashSequence();
            GetComponent<Collider2D>().enabled = false; //Desactiva el trigger
            escombros.SetActive(false);
        }
    }
}
