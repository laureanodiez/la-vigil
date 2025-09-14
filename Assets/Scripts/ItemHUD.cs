// Assets/Scripts/ItemHUD.cs
using UnityEngine;
using UnityEngine.UI;

public class ItemHUD : MonoBehaviour
{
    [SerializeField] private Image iconImage; // arrastrar Image del prefab
    [SerializeField] private GameObject rootPanel; // panel que contiene el icon (enable/disable)
    [SerializeField] private string watchedItemId = "wood";

    void OnEnable()
    {
        InventoryManager.Instance.OnItemAdded += OnItemAdded;
        InventoryManager.Instance.OnItemRemoved += OnItemRemoved;
        Refresh();
    }

    void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemAdded -= OnItemAdded;
            InventoryManager.Instance.OnItemRemoved -= OnItemRemoved;
        }
    }

    void OnItemAdded(ItemData item, int newCount)
    {
        if (item == null) return;
        if (item.id == watchedItemId) Refresh();
    }

    void OnItemRemoved(ItemData item, int newCount)
    {
        if (item != null && item.id == watchedItemId) Refresh();
        else Refresh();
    }

    void Refresh()
    {
        int c = InventoryManager.Instance.Count(watchedItemId);
        if (c > 0)
        {
            // find the ItemData asset to get icon (if you want, you can cache a ref)
            // simplest path: require that ItemHUD.iconImage assigned at design time
            rootPanel.SetActive(true);
            // if you want to set icon dynamically, store itemData somewhere accessible
        }
        else
        {
            rootPanel.SetActive(false);
        }
    }
}
