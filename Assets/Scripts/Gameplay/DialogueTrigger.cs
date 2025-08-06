using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    public DialogueManager dialogueManager;  // arrastra en el Inspector

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other) {
        if (triggered) return;
        if (other.CompareTag("Player")) {
            triggered = true;
            dialogueManager.StartConversation();
            // opcional: desactivar el trigger para que no vuelva a disparar
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
