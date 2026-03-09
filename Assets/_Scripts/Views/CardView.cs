using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CardView : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler {
    // ─── Card Data ─────────────────────────────────────────────
    public CardData CardData { get; private set; }

    // ─── Placement State ───────────────────────────────────────
    public bool IsPlaced { get; private set; } = false;
    public Vector2Int PlacedOrigin { get; private set; }
    public CardOrientation Orientation { get; private set; } = CardOrientation.Vertical;

    // ─── Interaction State ─────────────────────────────────────
    public bool IsHovering { get; private set; }
    public bool IsDragging { get; private set; }
    public bool IsSelected { get; private set; }
    [HideInInspector] public bool WasDragged;

    // ─── References ───────────────────────────────────────────
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Transform _handContainer;
    private Transform _cardsContainer;

    // private Transform _originalParent = null;
    private List<Slot> _occupiedSlots = new List<Slot>();

    private SlotView _lastHoveredSlot;
    private SlotView _currentTargetStartSlot; // ช่องที่เราเล็งไว้ปัจจุบัน

    [SerializeField] private GameObject _ghostPrefab; // ลาก Prefab Ghost มาใส่ใน Inspector
    private GameObject _currentGhost;

    [Header("Selection")]
    [SerializeField] public float SelectionOffset = 50f;

    // ─── Events ────────────────────────────────────────────────
    // CardHolder และ CardVisual subscribe event เหล่านี้
    [HideInInspector] public UnityEvent<CardView> OnPointerEnterEvent = new();
    [HideInInspector] public UnityEvent<CardView> OnPointerExitEvent = new();
    [HideInInspector] public UnityEvent<CardView> OnPointerDownEvent = new();
    [HideInInspector] public UnityEvent<CardView> OnPointerUpEvent = new();   // bool = isLongPress
    [HideInInspector] public UnityEvent<CardView> OnBeginDragEvent = new();
    [HideInInspector] public UnityEvent<CardView> OnEndDragEvent = new();
    [HideInInspector] public UnityEvent<CardView, bool> OnSelectEvent = new();      // bool = isSelected
    [HideInInspector] public UnityEvent<CardView> OnOrientationChangedEvent = new();
    [HideInInspector] public UnityEvent<CardView> OnPlacedEvent = new();
    [HideInInspector] public UnityEvent<CardView> OnRemovedFromGridEvent = new();

    private void Awake() {
        _canvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
        _handContainer = GameObject.Find("HandView").transform;
        _cardsContainer = GameObject.Find("CardsContainer").transform;
        
    }

    void Update() {
        // ตรวจจับการกดปุ่ม R เฉพาะตอนที่กำลังลากการ์ดอยู่เท่านั้น
        if (_canvasGroup.blocksRaycasts == false && Keyboard.current.rKey.wasPressedThisFrame) {
            RotateCard();
        }

    }

    public void Setup(CardData data) {
        CardData = data;
    }

    public void NotifyPointerExitSlot(SlotView slot) {
        // ถ้าเมาส์ออกจาก Slot ที่เราเคยบันทึกไว้ล่าสุดจริงๆ
        if (_lastHoveredSlot == slot) {
            _lastHoveredSlot = null;
            _currentTargetStartSlot = null; // ล้างเป้าหมาย Snap ด้วย
            HideGhost();
        }
    }

    private void ToggleOrientation() {
        Orientation = Orientation == CardOrientation.Horizontal
            ? CardOrientation.Vertical
            : CardOrientation.Horizontal;

        OnOrientationChangedEvent.Invoke(this);
    }
    
    public void SetOrientation(CardOrientation orientation) {
        if (Orientation == orientation) return;
        Orientation = orientation;
        OnOrientationChangedEvent.Invoke(this);
    }

    private void RotateCard() {
        ToggleOrientation();
        float angle = Orientation == CardOrientation.Horizontal ? -90f : 0f;
        _rectTransform.localRotation = Quaternion.Euler(0, 0, angle);

        if (_lastHoveredSlot != null) ShowGhost(_lastHoveredSlot);
    }

    // public void ReturnToHand() {
    //     Orientation = CardOrientation.Vertical;
    //     _rectTransform.localRotation = Quaternion.identity;
    //     transform.SetParent(_handContainer);
    // }

    public void ShowGhost(SlotView hoveredSlot) {
        _lastHoveredSlot = hoveredSlot;

        if (_currentTargetStartSlot == null) {
            DetermineInitialSlot(hoveredSlot);
        } else {
            // Hysteresis Logic: ถ้าเมาส์ยังวนเวียนในเขตการจองเดิม ไม่ต้องคำนวณใหม่
            SlotView secondOccupied = GridView.Instance.GetSecondSlotView(_currentTargetStartSlot, Orientation);
            if (hoveredSlot != _currentTargetStartSlot && hoveredSlot != secondOccupied) {
                DetermineInitialSlot(hoveredSlot);
            }
        }
        UpdateGhostPosition();
    }

    private void DetermineInitialSlot(SlotView hoveredSlot) {
        // ใช้ตำแหน่งกึ่งกลางการ์ดเทียบกับกึ่งกลาง Slot ตามที่คุณต้องการ
        float cardX = transform.position.x;
        float slotX = hoveredSlot.transform.position.x;
        float cardY = transform.position.y;
        float slotY = hoveredSlot.transform.position.y;

        if (Orientation == CardOrientation.Horizontal) {
            // แนวนอน: ถ้ากางการ์ดอยู่ขวาของ Slot ให้ช่องนี้เป็นช่องซ้าย
            if (cardX > slotX) {
                _currentTargetStartSlot = hoveredSlot;
            } else // ถ้าการ์ดอยู่ซ้าย ให้ถอยไปจองช่องทางซ้าย (เพื่อให้ช่องที่เมาส์ชี้เป็นช่องขวา)
              {
                Slot left = GridSystem.Instance.GetSlot(hoveredSlot.GridPosition.x - 1, hoveredSlot.GridPosition.y);
                _currentTargetStartSlot = (left != null && !left.IsOccupied)
                    ? GridView.Instance.GetSlotView(left.Position)
                    : hoveredSlot;
            }
        } else {
            // แนวตั้ง: ถ้ากางการ์ดอยู่สูงกว่ากึ่งกลาง Slot ให้ถอยขึ้นไปจองช่องบน
            if (cardY > slotY) {
                Slot above = GridSystem.Instance.GetSlot(hoveredSlot.GridPosition.x, hoveredSlot.GridPosition.y - 1);
                _currentTargetStartSlot = (above != null && !above.IsOccupied) 
                    ? GridView.Instance.GetSlotView(above.Position)
                    : hoveredSlot;
            } else {
                _currentTargetStartSlot = hoveredSlot;
            }
        }
    }

    private void UpdateGhostPosition() {
        if (_currentTargetStartSlot == null) return;

        Slot s1 = GridSystem.Instance.GetSlot(_currentTargetStartSlot.GridPosition);
        Slot s2 = GridSystem.Instance.GetSecondSlot(s1, Orientation);

        SlotView s1v = GridView.Instance.GetSlotView(_currentTargetStartSlot.GridPosition);
        SlotView s2v = GridView.Instance.GetSecondSlotView(s1v, Orientation);

        bool canPlace = !s1.IsOccupied && s2 != null && !s2.IsOccupied;

        if (canPlace) {
            if (_currentGhost == null) {
                _currentGhost = Instantiate(_ghostPrefab, _canvas.transform);
                // currentGhost.GetComponent<CanvasGroup>().blocksRaycasts = false; // กันกระพริบ
            }

            _currentGhost.SetActive(true);
            _currentGhost.transform.localRotation = _rectTransform.localRotation;

            // Snap ตรงกลาง
            Vector3 targetPos = (s2 != null)
                ? (s1v.transform.position + s2v.transform.position) / 2f
                : s1v.transform.position;
                
            _currentGhost.transform.position = targetPos;
        } else {
            HideGhost();
        }
    }

    public void HideGhost() {
        if (_currentGhost != null) _currentGhost.SetActive(false);
        // อย่าเพิ่งสั่ง currentTargetTopSlot = null ทันที เพื่อเก็บความจำ (Hysteresis)
    }

    // ฟังก์ชันให้ Slot เรียกเพื่อบันทึกการจอง
    public void SetCurrentSlots(params Slot[] slots) {
        _occupiedSlots.Clear();
        _occupiedSlots.AddRange(slots);
    }

    // public bool CanFit(SlotView startSlot) {
    //     // หา Slot ที่อยู่ข้างล่าง (พิกัด Y + 1)
    //     SlotView bottomSlot = GridSystem.Instance.GetSlot(startSlot.Position.x, startSlot.Position.y + 1);
    //     return bottomSlot != null && !bottomSlot.IsOccupied;
    // }

    public List<Slot> GetOccupiedSlots() {
        return _occupiedSlots;
    }

    public void PlaceInSlots(List<Slot> slots) {
        _occupiedSlots = new List<Slot>(slots);

        foreach (Slot s in _occupiedSlots) {
            s.PlaceCard(this);
        }

        // จัดตำแหน่งการ์ดให้อยู่ตรงกลางกลุ่ม Slot
        Vector3 centerPos = Vector3.zero;
        foreach (Slot s in slots) {
            var slotView = GridView.Instance.GetSlotView(s.Position);
            centerPos += slotView.transform.position;
        }

        transform.position = centerPos / slots.Count;
        transform.SetParent(_cardsContainer);
        _canvasGroup.blocksRaycasts = true;
    }

    public List<SlotView> GetAllTargetSlots(SlotView anchorSlot) {
        List<SlotView> targetSlots = new List<SlotView>();
        if (anchorSlot == null) return targetSlots;

        targetSlots.Add(anchorSlot); // ช่องแรก (ที่เมาส์ชี้ หรือช่อง Anchor)

        int nextX = anchorSlot.GridPosition.x;
        int nextY = anchorSlot.GridPosition.y;

        if (Orientation == CardOrientation.Horizontal) nextX += 1; // ถ้าแนวนอน ช่องที่สองคือ X + 1
        else nextY += 1; // ถ้าแนวตั้ง ช่องที่สองคือ Y + 1

        SlotView secondSlot = GridView.Instance.GetSlotView(nextX, nextY);
        if (secondSlot != null) {
            targetSlots.Add(secondSlot);
        }
        return targetSlots;
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

    // ─── Pointer Events (Interface Implementations) ────────────
    public void OnPointerEnter(PointerEventData eventData) {
        IsHovering = true;
        OnPointerEnterEvent.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        IsHovering = false;
        OnPointerExitEvent.Invoke(this);
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        OnPointerDownEvent.Invoke(this);
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        OnPointerUpEvent.Invoke(this);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        foreach (Slot s in _occupiedSlots) {
            s.ClearCard(); // สั่งให้ Slot ว่าง
        }
        _occupiedSlots.Clear(); // ล้างรายชื่อ Slot ที่เคยทับ

        // _originalParent = transform.parent;
        // transform.SetParent(_canvas.transform);
        _canvasGroup.blocksRaycasts = false;  // ให้เมาส์ทะลุไปโดน Slot ข้างหลัง

        _lastHoveredSlot = null;
        _currentTargetStartSlot = null;
    }

    public void OnDrag(PointerEventData eventData) {
        _rectTransform.anchoredPosition += eventData.delta;
        // transform.position = Input.mousePosition; // ลากตามเมาส์

        if (_lastHoveredSlot != null) {
            ShowGhost(_lastHoveredSlot);
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        _canvasGroup.blocksRaycasts = true;
        HideGhost();

        if (_currentGhost != null) Destroy(_currentGhost);

        // if (transform.parent == _canvas.transform) {
        //     ReturnToHand();
        // }

        OnEndDragEvent.Invoke(this);
    }

    // ─── Helpers ───────────────────────────────────────────────
    /// <summary> index ของ slot parent ใน CardHolder </summary>
    public int ParentIndex() {
        return transform.parent.CompareTag("Slot")
            ? transform.parent.GetSiblingIndex()
            : 0;
    }

    /// <summary> จำนวน sibling การ์ดทั้งหมดในมือ </summary>
    public int SiblingCount() {
        return transform.parent.CompareTag("Slot")
            ? transform.parent.parent.childCount - 1
            : 0;
    }

    /// <summary> ตำแหน่งของการ์ดนี้ใน hand normalized 0-1 </summary>
    public float NormalizedPosition() {
        return transform.parent.CompareTag("Slot")
            ? ExtensionMethods.Remap(
                (float)ParentIndex(), 0,
                (float)(transform.parent.parent.childCount - 1),
                0, 1)
            : 0f;
    }

    private void OnDestroy() {
        // if (_visual != null)
        //     Destroy(_visual.gameObject);
    }
}
