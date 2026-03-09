using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandSystem : Singleton<HandSystem> {
    [Header("Hand Configuration")]
    [Tooltip("จำนวนการ์ดสูงสุดในมือ")]
    [SerializeField] private int _maxHandSize = 8;

    [Tooltip("จำนวนการ์ดที่จั่วตอนเริ่ม turn")]
    [SerializeField] private int _drawPerTurn = 5;

    [Header("Card Prefab")]
    [Tooltip("Prefab ของการ์ด (CardView component)")]
    [SerializeField] private GameObject _cardPrefab;

    [Header("Hand Container")]
    [Tooltip("Parent transform สำหรับวางการ์ดในมือ")]
    [SerializeField] private Transform _handContainer;
    [SerializeField] private HandView _handView;

    [Tooltip("Prefab ของ slot สำหรับวางการ์ด")]
    [SerializeField] private GameObject _cardSlotPrefab;

    [Header("Current State")]
    private List<CardView> _cardsInHand = new();

    // ─── Events ────────────────────────────────────────────────
    // [HideInInspector] public UnityEvent<CardView> OnCardDrawn = new();
    // [HideInInspector] public UnityEvent<CardView> OnCardPlayed = new();
    // [HideInInspector] public UnityEvent<CardView> OnCardDiscarded = new();
    // [HideInInspector] public UnityEvent OnHandChanged = new();

    // ─── Properties ────────────────────────────────────────────
    public int CardsInHand => _cardsInHand.Count;
    public int MaxHandSize => _maxHandSize;
    public bool IsHandFull => _cardsInHand.Count >= _maxHandSize;
    public List<CardView> Cards => new List<CardView>(_cardsInHand); // defensive copy

    // ─── Initialization ────────────────────────────────────────
    protected override void Awake() {
        base.Awake();
        InitializeHandSlots();
    }

    private void InitializeHandSlots() {
        if (_handContainer == null || _cardSlotPrefab == null) {
            Debug.LogWarning("HandContainer or SlotPrefab not set!");
            return;
        }

        // สร้าง slot ตามจำนวน max hand size
        for (int i = 0; i < _maxHandSize; i++) {
            GameObject slot = Instantiate(_cardSlotPrefab, _handContainer);
            slot.name = $"CardSlot_{i}";
        }

        // Debug.Log($"Created {_maxHandSize} hand slots");
    }

    // ─── Draw ──────────────────────────────────────────────────
    public CardView DrawCard() {
        if (IsHandFull) {
            Debug.LogWarning("Cannot draw: hand is full!");
            return null;
        }

        // จั่วการ์ดจาก DeckSystem
        CardData cardData = DeckSystem.Instance.DrawCard();
        if (cardData == null) {
            Debug.LogWarning("Cannot draw: deck is empty!");
            return null;
        }

        // สร้าง CardView
        CardView cardView = CreateCardView(cardData);
        if (cardView == null) return null;

        // ใน DrawCard() หลังจากสร้างการ์ดเสร็จ
        if (_handView != null) {
            _handView.AddCard(cardView);
        }

        // เพิ่มเข้ามือ
        _cardsInHand.Add(cardView);

        // ใส่ card เข้า slot ที่ว่าง
        Transform slot = GetNextEmptySlot();
        if (slot != null) {
            cardView.transform.SetParent(slot, false);
            cardView.transform.localPosition = Vector3.zero;
        }

        // OnCardDrawn.Invoke(cardView);
        // OnHandChanged.Invoke();

        // Debug.Log($"Drew {cardData.CardName} to hand ({_cardsInHand.Count}/{_maxHandSize})");
        return cardView;
    }

    public List<CardView> DrawCards(int count) {
        List<CardView> drawnCards = new();

        for (int i = 0; i < count; i++) {
            CardView card = DrawCard();
            if (card == null) break; // มือเต็มหรือ deck หมด
            drawnCards.Add(card);
        }

        return drawnCards;
    }

    [ContextMenu("Draw")]
    public void DrawStartOfTurn() {
        int cardsToDraw = Mathf.Min(_drawPerTurn, _maxHandSize - _cardsInHand.Count);
        DrawCards(cardsToDraw);
        Debug.Log($"Drew {cardsToDraw} cards for turn start");
    }

    // ─── Play ──────────────────────────────────────────────────
    public void PlayCard(CardView card) {
        if (card == null || !_cardsInHand.Contains(card)) {
            Debug.LogWarning("Cannot play card: not in hand!");
            return;
        }

        if (_handView != null) {
            _handView.RemoveCard(card);
        }

        _cardsInHand.Remove(card);
        // OnCardPlayed.Invoke(card);
        // OnHandChanged.Invoke();

        Debug.Log($"Played {card.CardData.CardName} from hand ({_cardsInHand.Count}/{_maxHandSize})");
    }

    // ─── Discard ───────────────────────────────────────────────
    public void DiscardCard(CardView card) {
        if (card == null || !_cardsInHand.Contains(card)) {
            Debug.LogWarning("Cannot discard card: not in hand!");
            return;
        }

        if (_handView != null) {
            _handView.RemoveCard(card);
        }

        _cardsInHand.Remove(card);

        // ทิ้งการ์ดกลับ discard pile
        // DeckSystem.Instance.DiscardCard(card.Data);

        // ทำลาย CardView GameObject
        Destroy(card.gameObject);

        // OnCardDiscarded.Invoke(card);
        // OnHandChanged.Invoke();

        Debug.Log($"Discarded {card.CardData.CardName} from hand ({_cardsInHand.Count}/{_maxHandSize})");
    }

    public void DiscardHand() {
        List<CardView> cardsToDiscard = new List<CardView>(_cardsInHand);
        foreach (CardView card in cardsToDiscard) {
            DiscardCard(card);
        }

        Debug.Log("Discarded entire hand");
    }

    // ─── Card Creation ─────────────────────────────────────────
    private CardView CreateCardView(CardData data) {
        if (_cardPrefab == null) {
            Debug.LogError("Card prefab not set!");
            return null;
        }

            // Debug.LogError("Card prefab!");
        GameObject cardObj = Instantiate(_cardPrefab, _handContainer);
        CardView cardView = cardObj.GetComponent<CardView>();

        if (cardView == null) {
            Debug.LogError("Card prefab doesn't have CardView component!");
            Destroy(cardObj);
            return null;
        }

        // TODO: Set card data และ visual
        cardView.Setup(data);

        return cardView;
    }

    // ─── Helpers ───────────────────────────────────────────────
    private Transform GetNextEmptySlot() {
        if (_handContainer == null) return null;

        foreach (Transform slot in _handContainer) {
            // เช็คว่า slot นี้มีการ์ดอยู่แล้วหรือยัง
            if (slot.childCount == 0) {
                return slot;
            }
        }

        Debug.LogWarning("No empty slot found!");
        return null;
    }

    // ─── Debug ─────────────────────────────────────────────────
    /// <summary>
    /// แสดงการ์ดในมือ (debug)
    /// </summary>
    public void PrintHand() {
        Debug.Log($"=== HAND ({_cardsInHand.Count}/{_maxHandSize}) ===");
        foreach (CardView card in _cardsInHand) {
            Debug.Log($"  - {card.CardData.CardName}");
        }
    }
}
