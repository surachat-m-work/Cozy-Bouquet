using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    private CardView _cardView;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;

    // ─── Card Data ─────────────────────────────────────────────
    public CardData CardData { get; private set; }

    // ─── Placement State ───────────────────────────────────────
    public bool IsPlaced { get; private set; } = false;
    public Vector2Int PlacedOrigin { get; private set; }
    public CardOrientation Orientation { get; private set; } = CardOrientation.Horizontal;

    private Transform originalParent = null;
    private List<Slot> occupiedSlots = new List<Slot>();

    public Slot LastHoveredSlot { get; private set; }
    public Slot CurrentTargetStartSlot { get; private set; } // ช่องบนสุดที่เราเล็งไว้ปัจจุบัน

    private void Awake() {
        _cardView = GetComponent<CardView>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(CardData data) {
        CardData = data;
    }

    public void NotifyPointerExitSlot(Slot slot) {
        // ถ้าเมาส์ออกจาก Slot ที่เราเคยบันทึกไว้ล่าสุดจริงๆ
        if (LastHoveredSlot == slot) {
            LastHoveredSlot = null;
            CurrentTargetStartSlot = null; // ล้างเป้าหมาย Snap ด้วย
            _cardView.HideGhost();
        }
    }

    // ฟังก์ชันให้ Slot เรียกเพื่อบันทึกการจอง
    public void SetCurrentSlots(params Slot[] slots) {
        occupiedSlots.Clear();
        occupiedSlots.AddRange(slots);
    }

    public bool CanFit(Slot startSlot) {
        // หา Slot ที่อยู่ข้างล่าง (พิกัด Y + 1)
        // ตรงนี้คุณต้องมีระบบอ้างอิง Slot ตามพิกัด X,Y (เช่น Dictionary)
        Slot bottomSlot = GridSystem.Instance.GetSlot(startSlot.Position.x, startSlot.Position.y + 1);
        return bottomSlot != null && !bottomSlot.IsOccupied;
    }

    private Slot GetSecondSlot(Slot startSlot) {
        if (Orientation == CardOrientation.Horizontal)
            return GridSystem.Instance.GetSlot(startSlot.Position.x + 1, startSlot.Position.y);
        else
            return GridSystem.Instance.GetSlot(startSlot.Position.x, startSlot.Position.y + 1);
    }

    public void SetLastHoverSlot(Slot slot) {
        LastHoveredSlot = slot;
    }

    public void SetOrientation(CardOrientation orientation) {
        Orientation = orientation;
    }

    public void ToggleOrientation() {
        Orientation = Orientation == CardOrientation.Horizontal ? CardOrientation.Vertical : CardOrientation.Horizontal;
    }

    public void SetCurrentTargetStartSlot(Slot slot) {
        CurrentTargetStartSlot = slot;
    }

    public List<Slot> GetOccupiedSlots() {
        return occupiedSlots;
    }

    public void PlaceInSlots(List<Slot> slots) {
        occupiedSlots = new List<Slot>(slots);

        foreach (Slot s in occupiedSlots) {
            s.PlaceCard(this);
        }

        // จัดตำแหน่งการ์ดให้อยู่ตรงกลางกลุ่ม Slot
        Vector3 centerPos = Vector3.zero;
        foreach (Slot s in slots) centerPos += s.transform.position;
        transform.position = centerPos / slots.Count;
        // transform.SetParent(slots[0].transform); // เกาะไว้กับ Slot แรก

        transform.SetParent(_cardsContainer);
        _canvasGroup.blocksRaycasts = true;
    }

    // ใน DraggableCard.cs
    public List<Slot> GetAllTargetSlots(Slot anchorSlot) {
        List<Slot> targetSlots = new List<Slot>();
        if (anchorSlot == null) return targetSlots;

        targetSlots.Add(anchorSlot); // ช่องแรก (ที่เมาส์ชี้ หรือช่อง Anchor)

        int nextX = anchorSlot.Coordinate.x;
        int nextY = anchorSlot.Coordinate.y;

        if (isHorizontal) nextX += 1; // ถ้าแนวนอน ช่องที่สองคือ X + 1
        else nextY += 1; // ถ้าแนวตั้ง ช่องที่สองคือ Y + 1

        Slot secondSlot = GridSystem.Instance.GetSlotAt(nextX, nextY);
        if (secondSlot != null) {
            targetSlots.Add(secondSlot);
        }
        return targetSlots;
    }

    public void ShowGhost(Slot hoveredSlot) {
        _cardView.ShowGhost(hoveredSlot);
    }

    // ─── Placement ─────────────────────────────────────────────
    public void SetPlacement(Vector2Int origin, CardOrientation orientation) {
        PlacedOrigin = origin;
        Orientation = orientation;
        IsPlaced = true;
    }

    public void ClearPlacement() {
        IsPlaced = false;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        foreach (Slot s in occupiedSlots) {
            s.ClearCard(); // สั่งให้ Slot ว่าง
        }
        occupiedSlots.Clear(); // ล้างรายชื่อ Slot ที่เคยทับ

        originalParent = transform.parent;
        transform.SetParent(_canvas.transform);
        _canvasGroup.blocksRaycasts = false;  // ให้เมาส์ทะลุไปโดน Slot ข้างหลัง

        LastHoveredSlot = null;
        CurrentTargetStartSlot = null;
    }

    public void OnDrag(PointerEventData eventData) {
        _rectTransform.anchoredPosition += eventData.delta;
        // transform.position = Input.mousePosition; // ลากตามเมาส์

        if (LastHoveredSlot != null) {
            _cardView.ShowGhost(LastHoveredSlot);
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        _canvasGroup.blocksRaycasts = true;
        _cardView.HideGhost();

        if (_cardView.CurrentGhost != null) Destroy(_cardView.CurrentGhost);

        if (transform.parent == _canvas.transform) {
            _cardView.ReturnToHand();
        }
    }

}
