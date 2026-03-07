using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CodeLine : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string text;
    private PuzzleManager manager;
    private RectTransform rect;
    private Canvas canvas;

    private Transform originalParent;
    private Vector3 originalPosition;

    private Vector3 dragOffset;

    public void Setup(string t, PuzzleManager m)
    {
        text = t;
        manager = m;

        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        GetComponentInChildren<TMP_Text>().text = t;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = rect.localPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        dragOffset = rect.localPosition - (Vector3)localPoint;

        rect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pos
        );

        rect.localPosition = (Vector3)pos + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        foreach (Transform slot in manager.targetArea)
        {
            RectTransform slotRect = slot as RectTransform;

            if (RectTransformUtility.RectangleContainsScreenPoint(
                slotRect,
                eventData.position,
                eventData.pressEventCamera))
            {
                // SLOT BOŞSA NORMAL YERLEŞTİR
                if (slot.childCount == 0)
                {
                    transform.SetParent(slot);
                    rect.localPosition = Vector3.zero;
                    manager.CheckTargetSlots();
                    return;
                }
                else
                {
                    // SLOT DOLU → YER DEĞİŞTİR
                    Transform other = slot.GetChild(0);

                    other.SetParent(originalParent);
                    other.localPosition = originalPosition;

                    transform.SetParent(slot);
                    rect.localPosition = Vector3.zero;

                    manager.CheckTargetSlots();
                    return;
                }
            }
        }

        // Hiçbir slota düşmediyse geri dön
        transform.SetParent(originalParent);
        rect.localPosition = originalPosition;
    }
}
