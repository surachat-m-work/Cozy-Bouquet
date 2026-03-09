using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandView : MonoBehaviour, IDropHandler {
    [Header("Card Management")]
    private List<CardView> _cards = new();
    private CardView _selectedCard;
    private CardView _hoveredCard;
    private bool _isCrossing = false;

    private RectTransform _rect;

    // ─── Initialization ────────────────────────────────────────
    private void Awake() {
        _rect = GetComponent<RectTransform>();
    }

    private void Start() {
        RefreshCards();
        SubscribeToCards();

        // อัพเดท visual index หลังจาก frame แรก
        StartCoroutine(DelayedVisualUpdate());
    }

    private IEnumerator DelayedVisualUpdate() {
        yield return new WaitForSeconds(0.1f);
        // UpdateAllVisualIndexes();
    }

    // ─── Card Management ───────────────────────────────────────
    public void RefreshCards() {
        _cards.Clear();

        // หาการ์ดทั้งหมดที่เป็น child ของ slots
        foreach (Transform slot in transform) {
            CardView card = slot.GetComponentInChildren<CardView>();
            if (card != null) {
                _cards.Add(card);
            }
        }

        Debug.Log($"CardHolder: Found {_cards.Count} cards");
    }

    private void SubscribeToCards() {
        foreach (CardView card in _cards) {
            card.OnPointerEnterEvent.AddListener(OnCardPointerEnter);
            card.OnPointerExitEvent.AddListener(OnCardPointerExit);
            card.OnBeginDragEvent.AddListener(OnCardBeginDrag);
            card.OnEndDragEvent.AddListener(OnCardEndDrag);
        }
    }

    public void AddCard(CardView card) {
        if (card == null || _cards.Contains(card)) return;

        _cards.Add(card);

        // Subscribe events
        card.OnPointerEnterEvent.AddListener(OnCardPointerEnter);
        card.OnPointerExitEvent.AddListener(OnCardPointerExit);
        card.OnBeginDragEvent.AddListener(OnCardBeginDrag);
        card.OnEndDragEvent.AddListener(OnCardEndDrag);

        // UpdateAllVisualIndexes();
    }

    public void RemoveCard(CardView card) {
        if (card == null || !_cards.Contains(card)) return;

        _cards.Remove(card);

        // Unsubscribe events
        card.OnPointerEnterEvent.RemoveListener(OnCardPointerEnter);
        card.OnPointerExitEvent.RemoveListener(OnCardPointerExit);
        card.OnBeginDragEvent.RemoveListener(OnCardBeginDrag);
        card.OnEndDragEvent.RemoveListener(OnCardEndDrag);

        // UpdateAllVisualIndexes();
    }

    // ─── Event Handlers ────────────────────────────────────────
    private void OnCardPointerEnter(CardView card) {
        _hoveredCard = card;
    }

    private void OnCardPointerExit(CardView card) {
        if (_hoveredCard == card)
            _hoveredCard = null;
    }

    private void OnCardBeginDrag(CardView card) {
        _selectedCard = card;
    }

    private void OnCardEndDrag(CardView card) {
        if (_selectedCard == null) return;

        // Return การ์ดกลับตำแหน่งเดิม
        Vector3 targetPos = _selectedCard.IsSelected
            ? new Vector3(0, _selectedCard.SelectionOffset, 0)
            : Vector3.zero;

        _selectedCard.transform.localPosition = targetPos;

        // Force layout update (trick เล็กๆ ให้ layout group recalculate)
        _rect.sizeDelta += Vector2.right;
        _rect.sizeDelta -= Vector2.right;

        _selectedCard = null;
    }

    // ─── Swap Logic ────────────────────────────────────────────
    private void Update() {
        HandleSwap();
        // HandleDeleteKey();
        // HandleDeselectAll();
    }

    private void HandleSwap() {
        if (_selectedCard == null || _isCrossing) return;

        for (int i = 0; i < _cards.Count; i++) {
            CardView otherCard = _cards[i];
            if (otherCard == _selectedCard) continue;

            // เช็คว่าการ์ดที่ลากข้ามการ์ดอื่นหรือไม่
            bool crossedRight = _selectedCard.transform.position.x > otherCard.transform.position.x
                                && _selectedCard.ParentIndex() < otherCard.ParentIndex();

            bool crossedLeft = _selectedCard.transform.position.x < otherCard.transform.position.x
                               && _selectedCard.ParentIndex() > otherCard.ParentIndex();

            if (crossedRight || crossedLeft) {
                SwapCards(i);
                break;
            }
        }
    }

    private void SwapCards(int targetIndex) {
        _isCrossing = true;

        Transform selectedParent = _selectedCard.transform.parent;
        Transform targetParent = _cards[targetIndex].transform.parent;

        // Swap parents
        _cards[targetIndex].transform.SetParent(selectedParent);
        _cards[targetIndex].transform.localPosition = _cards[targetIndex].IsSelected
            ? new Vector3(0, _cards[targetIndex].SelectionOffset, 0)
            : Vector3.zero;

        _selectedCard.transform.SetParent(targetParent);

        _isCrossing = false;

        // Play swap animation บน CardVisual
        // CardVisual targetVisual = _cards[targetIndex].GetComponentInChildren<CardVisual>();
        // if (targetVisual != null) {
        //     bool swapIsRight = _cards[targetIndex].ParentIndex() > _selectedCard.ParentIndex();
        //     targetVisual.PlaySwapAnimation(swapIsRight ? -1 : 1);
        // }

        // อัพเดท visual indexes
        // UpdateAllVisualIndexes();
    }

    // ─── Visual Updates ────────────────────────────────────────
    // private void UpdateAllVisualIndexes() {
    //     foreach (CardView card in _cards) {
    //         CardVisual visual = card.GetComponentInChildren<CardVisual>();
    //         if (visual != null) {
    //             visual.UpdateIndex();
    //         }
    //     }
    // }

    // ─── Keyboard Input ────────────────────────────────────────
    // private void HandleDeleteKey() {
    //     if (Input.GetKeyDown(KeyCode.Delete) && _hoveredCard != null) {
    //         Debug.Log($"Deleting card: {_hoveredCard.Data?.CardName}");

    //         Transform slotParent = _hoveredCard.transform.parent;
    //         Destroy(slotParent.gameObject); // ลบทั้ง slot

    //         _cards.Remove(_hoveredCard);
    //         _hoveredCard = null;

    //         UpdateAllVisualIndexes();
    //     }
    // }

    /// <summary>
    /// คลิกขวาเพื่อ deselect การ์ดทั้งหมด
    /// </summary>
    // private void HandleDeselectAll() {
    //     if (Input.GetMouseButtonDown(1)) { // Right mouse button
    //         foreach (CardView card in _cards) {
    //             card.Deselect();
    //         }
    //     }
    // }

    // ─── Public Utilities ──────────────────────────────────────
    /// <summary>
    /// ดึงการ์ดทั้งหมดใน holder
    /// </summary>
    public List<CardView> GetAllCards() {
        return new List<CardView>(_cards); // defensive copy
    }

    /// <summary>
    /// ดึงการ์ดที่ถูกเลือกอยู่
    /// </summary>
    public List<CardView> GetSelectedCards() {
        return _cards.Where(c => c.IsSelected).ToList();
    }

    /// <summary>
    /// จำนวนการ์ดใน holder
    /// </summary>
    public int CardCount => _cards.Count;

    // ─── Debug ─────────────────────────────────────────────────
    [ContextMenu("Refresh Cards")]
    private void DebugRefreshCards() {
        RefreshCards();
        Debug.Log($"Refreshed: {_cards.Count} cards found");
    }

    [ContextMenu("Print All Cards")]
    private void DebugPrintCards() {
        Debug.Log($"=== CARDS IN HOLDER ({_cards.Count}) ===");
        for (int i = 0; i < _cards.Count; i++) {
            Debug.Log($"{i}: {_cards[i].CardData?.CardName} (Parent Index: {_cards[i].ParentIndex()})");
        }
    }

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
