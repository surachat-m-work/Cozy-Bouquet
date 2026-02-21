using UnityEngine;
using UnityEngine.EventSystems;

public class GridView : MonoBehaviour, IPointerEnterHandler {
    public void OnPointerEnter(PointerEventData eventData) {
        if (eventData.pointerDrag != null) {
            CardView card = eventData.pointerDrag.GetComponent<CardView>();
            // ถ้าเมาส์กลับมาที่พื้นหลัง (แต่ไม่ได้อยู่บน Slot) ให้ซ่อน Ghost ทันที
            if (card != null) {
                card.HideGhost();
            }
        }
    }
}