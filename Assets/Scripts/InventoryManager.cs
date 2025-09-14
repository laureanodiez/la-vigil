// Assets/Scripts/InventoryManager.cs
using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // estado simple: cantidad por id (puede ampliarse)
    private Dictionary<string,int> items = new Dictionary<string,int>();

    // eventos para que UI y sistemas reaccionen
    public event Action<ItemData, int> OnItemAdded;     // (item, newCount)
    public event Action<ItemData, int> OnItemRemoved;   // (item, newCount)

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Añadir por ItemData
    public void Add(ItemData item, int amount = 1)
    {
        if (item == null) return;
        if (!items.ContainsKey(item.id)) items[item.id] = 0;
        items[item.id] += amount;

        OnItemAdded?.Invoke(item, items[item.id]);
    }

    // Query
    public bool Has(string itemId, int required = 1)
    {
        if (!items.ContainsKey(itemId)) return false;
        return items[itemId] >= required;
    }

    // Consumir
    public bool Use(string itemId, int amount = 1)
    {
        if (!Has(itemId, amount)) return false;
        items[itemId] -= amount;
        if (items[itemId] <= 0) items.Remove(itemId);
        // Para invocar OnItemRemoved necesitamos el ItemData; en general mantiene referencias
        // Puedes mantener un registro de ItemData por id o pasar el ItemData cuando uses.
        OnItemRemoved?.Invoke(null, items.ContainsKey(itemId) ? items[itemId] : 0);
        return true;
    }

    // Helper para llamar Use por ItemData
    public bool Use(ItemData item, int amount = 1)
    {
        if (item == null) return false;
        bool ok = Use(item.id, amount);
        if (ok) OnItemRemoved?.Invoke(item, items.ContainsKey(item.id) ? items[item.id] : 0);
        return ok;
    }

    // Consulta de cantidad
    public int Count(string itemId)
    {
        if (!items.ContainsKey(itemId)) return 0;
        return items[itemId];
    }
}
