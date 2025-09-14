// Assets/Scripts/LadderRepair.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LadderRepair : MonoBehaviour
{
    public string requiredItemId = "wood";
    public ItemData requiredItemData;       // opcional, para pasar al InventoryManager events
    public Sprite repairedSprite;           // sprite que pone cuando se arregla
    public SpriteRenderer targetRenderer;   // el renderer cuyo sprite cambiará
    public bool autoUseOnTrigger = false;   // si true, al paso intenta usar sin botón
    public string playerTag = "Player";
    public AudioClip repairSfx;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (autoUseOnTrigger)
            TryUse(other.gameObject);
    }

    // Llamar desde PlayerInteraction (cuando presiona acción) o desde trigger si autoUseOnTrigger
    public bool TryUse(GameObject user)
    {
        if (!InventoryManager.Instance.Has(requiredItemId)) return false;

        // consumir
        InventoryManager.Instance.Use(requiredItemData != null ? requiredItemData : null);

        // cambiar sprite
        if (targetRenderer != null && repairedSprite != null)
            targetRenderer.sprite = repairedSprite;

        if (repairSfx != null) AudioSource.PlayClipAtPoint(repairSfx, transform.position);

        // opcional: desactivar el collider para que no pueda repararse otra vez
        GetComponent<Collider2D>().enabled = false;
        return true;
    }
}
