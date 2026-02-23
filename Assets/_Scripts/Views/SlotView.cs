using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Visual representation ของ slot แต่ละช่องบน grid
/// แสดงสี highlight และรับ drop event จากการ์ด
/// </summary>
public class SlotView : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler {

    [Header("Slot Data")]
    [SerializeField] private Vector2Int _gridPosition;
    [SerializeField] private SlotType _slotType;

    [Header("Visual")]
    [SerializeField] private Image _slotImage;
    [SerializeField] private Image _highlightImage;

    [Header("Colors")]
    [SerializeField] private Color _colorOpen = new Color(1f, 1f, 1f, 0.1f);
    [SerializeField] private Color _colorBorder = new Color(0.3f, 0.6f, 1f, 0.2f);
    [SerializeField] private Color _colorShade = new Color(0.7f, 0.5f, 0.9f, 0.2f);
    [SerializeField] private Color _colorBloom = new Color(1f, 0.8f, 0.2f, 0.3f);

    [SerializeField] private Color _colorHighlight = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private Color _colorValid = new Color(0.3f, 1f, 0.3f, 0.5f);
    [SerializeField] private Color _colorInvalid = new Color(1f, 0.3f, 0.3f, 0.5f);

    [Header("Animation")]
    [SerializeField] private float _highlightDuration = 0.2f;

    // ─── State ─────────────────────────────────────────────────
    private bool _isHovering;
    private bool _isOccupied = false; // เช็คว่ามีใครวางอยู่หรือยัง
    private Card _draggingCard;

    // ─── Properties ────────────────────────────────────────────
    public Vector2Int GridPosition => _gridPosition;
    public SlotType SlotType => _slotType;

    // ─── Initialization ────────────────────────────────────────
    public void Initialize(Vector2Int position, SlotType type) {
        _gridPosition = position;
        _slotType = type;

        if (_slotImage == null)
            _slotImage = GetComponent<Image>();

        if (_highlightImage == null) {
            // สร้าง highlight image ถ้ายังไม่มี
            GameObject highlightObj = new GameObject("Highlight");
            highlightObj.transform.SetParent(transform, false);

            RectTransform highlightRect = highlightObj.AddComponent<RectTransform>();
            highlightRect.anchorMin = Vector2.zero;
            highlightRect.anchorMax = Vector2.one;
            highlightRect.sizeDelta = Vector2.zero;

            _highlightImage = highlightObj.AddComponent<Image>();
            _highlightImage.raycastTarget = false;
        }

        UpdateSlotColor();
        // HideHighlight();
    }

    // ─── Visual Updates ────────────────────────────────────────
    private void UpdateSlotColor() {
        if (_slotImage == null) return;

        _slotImage.color = _slotType switch {
            SlotType.Open => _colorOpen,
            SlotType.Border => _colorBorder,
            SlotType.Shade => _colorShade,
            SlotType.Bloom => _colorBloom,
            _ => _colorOpen
        };
    }

    // private void ShowHighlight(Color color) {
    //     if (_highlightImage == null) return;

    //     _highlightImage.DOKill();
    //     _highlightImage.color = color;
    //     _highlightImage.DOFade(color.a, _highlightDuration);
    // }

    // private void HideHighlight() {
    //     if (_highlightImage == null) return;

    //     _highlightImage.DOKill();
    //     _highlightImage.DOFade(0f, _highlightDuration);
    // }

    // /// <summary>
    // /// Highlight slot ที่ 2 ที่การ์ด 1x2 จะครอบ
    // /// </summary>
    // private void HighlightSecondSlot(CardView card, bool canPlace) {
    //     if (card == null) return;

    //     // คำนวณตำแหน่งของ slot ที่ 2
    //     Vector2Int secondSlotPos = GridSystem.Instance.GetSecondSlotPosition(_gridPosition, card.Orientation);

    //     // หา GridSlotView ของ slot ที่ 2
    //     GridView visualizer = FindFirstObjectByType<GridView>();
    //     if (visualizer == null) return;

    //     SlotView secondSlot = visualizer.GetSlotView(secondSlotPos);
    //     if (secondSlot != null) {
    //         secondSlot.ShowHighlight(canPlace ? _colorValid : _colorInvalid);
    //     }
    // }

    // /// <summary>
    // /// ซ่อน highlight ของ slot ที่ 2
    // /// </summary>
    // private void HideSecondSlotHighlight() {
    //     if (_draggingCard == null) return;

    //     // คำนวณตำแหน่งของ slot ที่ 2
    //     Vector2Int secondSlotPos = GridSystem.Instance.GetSecondSlotPosition(_gridPosition, _draggingCard.Orientation);

    //     // หา GridSlotView ของ slot ที่ 2
    //     GridView visualizer = FindFirstObjectByType<GridView>();
    //     if (visualizer == null) return;

    //     SlotView secondSlot = visualizer.GetSlotView(secondSlotPos);
    //     if (secondSlot != null) {
    //         secondSlot.HideHighlight();
    //     }
    // }

    // ─── Drag & Drop Events ────────────────────────────────────
    public void OnPointerEnter(PointerEventData eventData) {
        _isHovering = true;

        // เช็คว่ามีการ์ดกำลังถูก drag อยู่หรือไม่
        _draggingCard = eventData.pointerDrag?.GetComponent<Card>();

        if (_draggingCard != null) {
            _draggingCard.ShowGhost(this); /////////////////////////////////
            Debug.Log($"🎴 Card dragging: {_draggingCard.CardData?.CardName}");

            // เช็คว่าวางได้หรือไม่
            bool canPlace = CanPlaceCard(_draggingCard);
            Debug.Log($"📍 Can place: {canPlace}");

            // Highlight slot นี้
            // ShowHighlight(canPlace ? _colorValid : _colorInvalid);

            // Highlight slot ที่ 2 ด้วย (เพราะการ์ดขนาด 1x2)
            // HighlightSecondSlot(_draggingCard, canPlace);
        // } else {
            // Debug.Log($"🖱️ Just hovering, no card dragging");
            // ShowHighlight(_colorHighlight);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        // _draggingCard = eventData.pointerDrag?.GetComponent<CardView>();

        if (_draggingCard != null) {
            _draggingCard.NotifyPointerExitSlot(this);
        }

        _isHovering = false;
        _draggingCard = null;
        // HideHighlight();

        // ซ่อน highlight ของ slot ที่ 2 ด้วย
        // HideSecondSlotHighlight();
    }

    public void OnDrop(PointerEventData eventData) {
        CardView droppedCard = eventData.pointerDrag?.GetComponent<CardView>();
        if (droppedCard == null) return;

        // พยายามวางการ์ดลง grid
        bool success = TryPlaceCard(droppedCard);

        if (success) {
            Debug.Log($"Placed {droppedCard.CardData?.CardName} at {_gridPosition} ({_slotType})");

            // แจ้ง HandSystem ว่าการ์ดถูกเล่นแล้ว
            // HandSystem.Instance?.PlayCard(droppedCard);
        // } else {
        //     Debug.LogWarning($"Cannot place card at {_gridPosition}");
        }

        // HideHighlight();
    }

    // public void OnDrop(PointerEventData eventData) {
    //     CardView card = eventData.pointerDrag.GetComponent<CardView>();
    //     if (card == null) return;

    //     Slot anchor = card.GetCurrentTargetTopSlot();

    //     List<Slot> allTargetSlots = card.GetAllTargetSlots(anchor);

    //     bool canPlace = true;

    //     if (allTargetSlots.Count < 2) canPlace = false;

    //     foreach (Slot s in allTargetSlots) {
    //         if (s.isOccupied) canPlace = false;
    //     }

    //     if (canPlace) {
    //         card.PlaceInSlots(allTargetSlots);
    //     } else {
    //         card.ReturnToHand();
    //     }
    // }

    // ─── Placement Logic ───────────────────────────────────────
    private bool CanPlaceCard(CardView card) {
        if (card == null) return false;
        return GridSystem.Instance.CanPlaceCard(_gridPosition, card.Orientation);
    }

    private bool TryPlaceCard(CardView card) {
        if (card == null) return false;

        bool success = GridSystem.Instance.PlaceCard(card, _gridPosition, card.Orientation);

        if (success) {
            // ย้ายการ์ดมาเป็น child ของ Grid Container แทน Card Container
            GridView visualizer = FindFirstObjectByType<GridView>();
            if (visualizer != null && visualizer.CardContainer != null) {
                card.transform.SetParent(visualizer.CardContainer, true);
            }

            // ตั้งตำแหน่งการ์ดให้ตรงกับ slot
            PositionCardOnSlot(card);
        }

        return success;
    }

    /// <summary>
    /// วางการ์ดให้ตรงกับตำแหน่ง slot
    /// </summary>
    private void PositionCardOnSlot(CardView card) {
        if (card == null) return;

        // ตั้งตำแหน่ง CardView (UI element)
        RectTransform cardRect = card.GetComponent<RectTransform>();
        RectTransform slotRect = GetComponent<RectTransform>();

        if (cardRect != null && slotRect != null) {
            // คำนวณ offset ตาม orientation
            Vector2 offset = Vector2.zero;

            if (card.Orientation == CardOrientation.Horizontal) {
                // วางแนวนอน → เลื่อนไปขวา 0.5 slot
                offset = Vector2.right * (slotRect.rect.width * 0.5f);
            } else {
                // วางแนวตั้ง → เลื่อนลงล่าง 0.5 slot
                offset = Vector2.down * (slotRect.rect.height * 0.5f);
            }

            // Animate CardView ไปยังตำแหน่ง slot
            Vector2 targetAnchoredPos = slotRect.anchoredPosition + offset;
            cardRect.DOAnchorPos(targetAnchoredPos, 0.3f).SetEase(Ease.OutBack);
        }

        // Reset rotation และ scale
        card.transform.DORotate(Vector3.zero, 0.3f);
        card.transform.DOScale(Vector3.one * 0.8f, 0.3f); // เล็กลงนิดหน่อยให้พอดี slot

        // CardVisual จะ follow CardView อัตโนมัติใน Update()
    }

    // ─── Debug ─────────────────────────────────────────────────
    private void OnValidate() {
        if (_slotImage != null) {
            UpdateSlotColor();
        }
    }

    private void OnDrawGizmos() {
        // แสดงตำแหน่ง grid ใน Scene view
        Gizmos.color = _slotType switch {
            SlotType.Border => Color.blue,
            SlotType.Shade => Color.magenta,
            SlotType.Bloom => Color.yellow,
            _ => Color.white
        };

        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}