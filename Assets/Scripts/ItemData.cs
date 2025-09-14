// Assets/Scripts/ItemData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Vigil/ItemData", fileName = "NewItem")]
public class ItemData : ScriptableObject
{
    public string id;              // p.ej. "wood"
    public string displayName;
    public Sprite icon;            // ícono pequeño para HUD
    public bool consumable = true; // si true se borra al usar
}
