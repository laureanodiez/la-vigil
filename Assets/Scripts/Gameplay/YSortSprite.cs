using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class YSortSprite : MonoBehaviour
{
    public int sortingOffset = 0; // si querés forzar unos pasos por encima/abajo
    public int multiplier = 100; // precisión; 100 -> orden por 0.01 unidades

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateOrder();
    }

    void LateUpdate()
    {
        // Solo en play mode actualizamos por performance; pero Awake hace el valor inicial.
        if (Application.isPlaying)
            UpdateOrder();
    }

    void UpdateOrder()
    {
        // Orden: valor mayor --> render al final (por encima). 
        // Usamos -y para que más abajo (valor y pequeño) tengan mayor order (se ven adelante)
        int order = Mathf.RoundToInt(-transform.position.y * multiplier) + sortingOffset;
        if (sr.sortingOrder != order)
            sr.sortingOrder = order;
    }
}
