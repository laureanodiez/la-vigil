using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Referencias a objetos interactuables")]
    [SerializeField] private GameObject constancio;
    [SerializeField] private GameObject notaConstancioHallPasado;
    [SerializeField] private GameObject madera;

    [Header("Sonido de interacción (único)")]
    [SerializeField] private AudioClip interactSound;

    [Header("Ajustes")]
    [SerializeField, Tooltip("Distancia máxima para que ocurra la interacción automáticamente")]
    private float interactRange = 1.5f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Update()
    {
        // Chequea cada objeto: si está activo y Quimi está lo bastante cerca, lo desactiva y suena una vez.
        TryAutoInteract(constancio);
        TryAutoInteract(notaConstancioHallPasado);
        TryAutoInteract(madera);
    }

    private void TryAutoInteract(GameObject obj)
    {
        if (obj == null || !obj.activeSelf)
            return;

        float dist = Vector2.Distance(transform.position, obj.transform.position);
        if (dist <= interactRange)
        {
            // Suena una única vez
            if (interactSound != null)
                audioSource.PlayOneShot(interactSound);

            // Desactiva el objeto
            obj.SetActive(false);
        }
    }

    // Para visualizar en el Scene el rango de interacción:
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
