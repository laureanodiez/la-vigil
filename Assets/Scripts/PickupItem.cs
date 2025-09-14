// Assets/Scripts/PickupItem.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PickupItem : MonoBehaviour
{
    public ItemData item;               // asignar Item_Wood asset
    public bool autoPickupOnTrigger = true;
    public string playerTag = "Player";
    public AudioClip pickupSfx;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (autoPickupOnTrigger) DoPickup();
    }

    public void DoPickup()
    {
        if (item == null) { Debug.LogWarning("PickupItem: item null"); return; }

        InventoryManager.Instance.Add(item, 1);

        if (pickupSfx != null)
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        // desactivar/reusar: para persistencia/respawn preferís SetActive(false) o destroy según flujo
        gameObject.SetActive(false);
    }
}
