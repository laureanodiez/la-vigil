using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Canvas))]
public class MenuNavigation : MonoBehaviour
{
    [Tooltip("Primer elemento seleccionado al abrir el menú")]
    public Selectable defaultSelection;

    [Tooltip("Tiempo (s) entre lecturas repetidas de eje (mantener tecla/joystick)")]
    public float inputRepeatDelay = 0.18f;

    private float nextInputTime = 0f;

    void OnEnable()
    {
        // Seleccionar el default al abrir
        if (defaultSelection != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            defaultSelection.Select();
        }
    }

    void Update()
    {
        if (EventSystem.current == null) return;

        // Si no hay seleccionado, seleccionar el default
        if (EventSystem.current.currentSelectedGameObject == null && defaultSelection != null)
        {
            defaultSelection.Select();
        }

        // Lectura directa de teclado + joystick
        bool upPressed = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
        bool downPressed = Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
        bool leftPressed = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
        bool rightPressed = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        // Si el eje está activo y pasó el delay
        if (Mathf.Abs(v) > 0.5f || Mathf.Abs(h) > 0.5f)
        {
            if (Time.time >= nextInputTime)
            {
                if (v > 0.5f) upPressed = true;
                else if (v < -0.5f) downPressed = true;
                else if (h < -0.5f) leftPressed = true;
                else if (h > 0.5f) rightPressed = true;

                nextInputTime = Time.time + inputRepeatDelay;
            }
        }
        else
        {
            // resetear para permitir pulsos inmediatos si sueltas el stick
            nextInputTime = 0f;
        }

        // Navegar según input
        if (upPressed) Navigate(Vector2.up);
        if (downPressed) Navigate(Vector2.down);
        if (leftPressed) Navigate(Vector2.left);
        if (rightPressed) Navigate(Vector2.right);

        // Submit / Cancel
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitCurrent();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Cancel - simula botón cancel (ej: cerrar menú)
            // Puedes enlazar un método público que cierre el menú.
            ExecuteCancel();
        }
    }

    void Navigate(Vector2 dir)
    {
        var currentGO = EventSystem.current.currentSelectedGameObject;
        if (currentGO == null) return;

        var sel = currentGO.GetComponent<Selectable>();
        if (sel == null) return;

        Selectable next = null;
        if (dir == Vector2.up) next = sel.FindSelectableOnUp();
        else if (dir == Vector2.down) next = sel.FindSelectableOnDown();
        else if (dir == Vector2.left) next = sel.FindSelectableOnLeft();
        else if (dir == Vector2.right) next = sel.FindSelectableOnRight();

        if (next != null)
        {
            next.Select();
        }
        else
        {
            // opcional: wrap-around (si querés que vaya al primer/último)
            TryWrap(sel, dir);
        }
    }

    void TryWrap(Selectable sel, Vector2 dir)
    {
        // Implementación sencilla de wrap vertical: ir al primero/último hijo del mismo contenedor
        var parent = sel.transform.parent;
        if (parent == null) return;

        Selectable candidate = null;
        if (dir == Vector2.up)
        {
            // buscar el último selectable visible en el mismo parent
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var s = parent.GetChild(i).GetComponent<Selectable>();
                if (s != null && s.IsInteractable() && s.navigation.mode != Navigation.Mode.None) { candidate = s; break; }
            }
        }
        else if (dir == Vector2.down)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var s = parent.GetChild(i).GetComponent<Selectable>();
                if (s != null && s.IsInteractable() && s.navigation.mode != Navigation.Mode.None) { candidate = s; break; }
            }
        }

        if (candidate != null) candidate.Select();
    }

    void SubmitCurrent()
    {
        var go = EventSystem.current.currentSelectedGameObject;
        if (go == null) return;

        var btn = go.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.Invoke();
            return;
        }

        // otros casos: Toggle, Dropdown, etc.
        var toggle = go.GetComponent<Toggle>();
        if (toggle != null) toggle.isOn = !toggle.isOn;
    }

    void ExecuteCancel()
    {
        // búsqueda simple: un componente ICancelHandler (si se usa)
        var go = EventSystem.current.currentSelectedGameObject;
        // por ahora llamamos BroadcastMessage a root del menú para que lo cierre
        transform.SendMessage("OnMenuCancel", SendMessageOptions.DontRequireReceiver);
    }
}
