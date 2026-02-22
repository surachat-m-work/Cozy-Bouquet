using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // จำเป็นต้องใช้สำหรับ Drag & Drop

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler {
    
    [SerializeField] private SlotType _type;
    [SerializeField] private Vector2Int _coordinate;
    
    public SlotType Type => _type;
    public Vector2Int Coordinate => _coordinate; // พิกัด x, y ของช่องนี้

    public CardView OccupiedCard { get; private set; }
    public bool isOccupied = false; // เช็คว่ามีใครวางอยู่หรือยัง

    public void OnPointerEnter(PointerEventData eventData) {
        // เช็คว่าเรากำลังลากการ์ดอยู่หรือไม่
        if (eventData.pointerDrag != null && !isOccupied) {
            CardView card = eventData.pointerDrag.GetComponent<CardView>();
            if (card != null) {
                // ส่งข้อมูลตำแหน่งนี้ไปให้การ์ดเพื่อโชว์ Ghost
                card.ShowGhost(this);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (eventData.pointerDrag != null) {
            CardView card = eventData.pointerDrag.GetComponent<CardView>();
            if (card != null) card.NotifyPointerExitSlot(this);
        }
    }

    public void OnDrop(PointerEventData eventData) {
        CardView card = eventData.pointerDrag.GetComponent<CardView>();
        if (card == null) return;

        // 1. หาช่องอ้างอิงที่การ์ดอยากจะลง (เหมือนเดิม)
        Slot anchor = card.GetCurrentTargetTopSlot();

        // 2. ถามการ์ดว่า "จากช่องเนี้ย นายต้องใช้กี่ช่องและช่องไหนบ้าง?"
        List<Slot> allTargetSlots = card.GetAllTargetSlots(anchor);

        // 3. ตรวจสอบเงื่อนไขการวาง (เช่น ต้องมี 2 ช่องครบสำหรับ 1x2 และต้องว่างทั้งคู่)
        bool canPlace = true;

        // ถ้าเป็นการ์ดใหญ่ แต่หาช่องที่สองไม่เจอ หรือช่องใดช่องหนึ่งไม่ว่าง
        if (allTargetSlots.Count < 2) canPlace = false;

        foreach (Slot s in allTargetSlots) {
            if (s.isOccupied) canPlace = false;
        }

        if (canPlace) {
            // วางลงไปในทุุกช่องที่หามาได้
            card.PlaceInSlots(allTargetSlots);
        } else {
            // ถ้าวางไม่ได้ ให้กลับที่เดิม
            card.ReturnToHand();
        }
    }

    public void SetCard(CardView card) {
        OccupiedCard = card;
        isOccupied = true;
    }

    // ฟังก์ชันสำหรับเคลียร์พื้นที่
    public void ClearSlot() {
        OccupiedCard = null;
        isOccupied = false;
    }
}