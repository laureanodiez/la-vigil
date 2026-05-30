using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject tooltipObject;
    [SerializeField] private float delay = 1.0f; // Segundos antes de aparecer
    
    private Coroutine showCoroutine;

    private void Start()
    {
        if (tooltipObject != null) tooltipObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        showCoroutine = StartCoroutine(ShowTooltipAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (showCoroutine != null) StopCoroutine(showCoroutine);
        if (tooltipObject != null) tooltipObject.SetActive(false);
    }

    private IEnumerator ShowTooltipAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        if (tooltipObject != null) tooltipObject.SetActive(true);
    }
}