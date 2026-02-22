using UnityEngine;
using UnityEngine.EventSystems;

public class HandView : MonoBehaviour, IDropHandler {
    public void OnDrop(PointerEventData eventData) {
        GameObject droppedCard = eventData.pointerDrag;
        if (droppedCard != null) {
            // ย้ายการ์ดกลับมาเป็นลูกของ HandContainer
            droppedCard.transform.SetParent(this.transform);
            
            // Layout Group จะจัดการตำแหน่งให้เอง เราไม่ต้องตั้ง Vector2.zero ก็ได้
            Debug.Log("Card returned to Hand");
        }
    }
}
