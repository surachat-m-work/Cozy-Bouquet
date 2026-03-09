using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckSystem : Singleton<DeckSystem> {
    [Header("Deck Configuration")]
    [SerializeField] private List<CardData> _starterDeck = new();

    private List<CardData> _drawPile = new();
    private List<CardData> _discardPile = new();

    // ─── Properties ────────────────────────────────────────────
    public int CardsInDeck => _drawPile.Count;
    public int CardsInDiscard => _discardPile.Count;
    public int TotalCards => _drawPile.Count + _discardPile.Count;

    // ─── Initialization ────────────────────────────────────────
    protected override void Awake() {
        base.Awake();
        InitializeDeck();
    }

    public void InitializeDeck() {
        _drawPile.Clear();
        _discardPile.Clear();

        // Copy starter deck
        _drawPile = new List<CardData>(_starterDeck);

        // สับการ์ด
        ShuffleDeck();
        // Debug.Log($"Deck initialized with {_drawPile.Count} cards");
    }

    public void ShuffleDeck() {
        // Fisher-Yates shuffle
        for (int i = _drawPile.Count - 1; i > 0; i--) {
            int randomIndex = Random.Range(0, i + 1);
            (_drawPile[i], _drawPile[randomIndex]) = (_drawPile[randomIndex], _drawPile[i]);
        }
        // Debug.Log($"Deck shuffled: {_drawPile.Count} cards");
    }

    public CardData DrawCard() {
        // ถ้า deck หมด แต่มี discard pile → สับ discard pile กลับเข้า deck
        if (_drawPile.Count == 0 && _discardPile.Count > 0) {
            ReshuffleDiscardIntoDeck();
        }

        // ถ้ายังหมดอยู่ → คืน null
        if (_drawPile.Count == 0) {
            // Debug.LogWarning("Cannot draw: deck is empty!");
            return null;
        }

        // จั่วการ์ดใบบนสุด
        CardData card = _drawPile[0];
        _drawPile.RemoveAt(0);

        // Debug.Log($"Drew card: {card.CardName} ({_drawPile.Count} cards left)");
        return card;
    }

    public List<CardData> DrawCards(int count) {
        List<CardData> drawnCards = new();

        for (int i = 0; i < count; i++) {
            CardData card = DrawCard();
            if (card == null) break; // deck หมด
            drawnCards.Add(card);
        }

        return drawnCards;
    }

    public void DiscardCard(CardData card) {
        if (card == null) return;

        _discardPile.Add(card);
        Debug.Log($"Discarded card: {card.CardName} ({_discardPile.Count} in discard pile)");
    }

    public void DiscardCards(List<CardData> cards) {
        foreach (CardData card in cards) {
            DiscardCard(card);
        }
    }

    public void ReshuffleDiscardIntoDeck() {
        Debug.Log($"Reshuffling {_discardPile.Count} cards from discard pile into deck");

        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
        ShuffleDeck();
    }

    public void AddCardToDeck(CardData card) {
        if (card == null) return;

        _drawPile.Add(card);
        Debug.Log($"Added {card.CardName} to deck");
    }

    public bool RemoveCardFromDeck(CardData card) {
        if (card == null) return false;

        // ลองหาใน draw pile ก่อน
        if (_drawPile.Remove(card)) {
            Debug.Log($"Removed {card.CardName} from draw pile");
            return true;
        }

        // ถ้าไม่เจอ ลองหาใน discard pile
        if (_discardPile.Remove(card)) {
            Debug.Log($"Removed {card.CardName} from discard pile");
            return true;
        }

        Debug.LogWarning($"Cannot remove {card.CardName}: not found in deck");
        return false;
    }

    // ─── Debug ─────────────────────────────────────────────────
    /// <summary>
    /// แสดงการ์ดทั้งหมดใน deck (debug)
    /// </summary>
    public void PrintDeckContents() {
        Debug.Log("=== DECK CONTENTS ===");
        Debug.Log($"Draw Pile ({_drawPile.Count}):");
        foreach (var card in _drawPile) {
            Debug.Log($"  - {card.CardName}");
        }
        Debug.Log($"Discard Pile ({_discardPile.Count}):");
        foreach (var card in _discardPile) {
            Debug.Log($"  - {card.CardName}");
        }
    }

    // ─── Save/Load (สำหรับอนาคต) ──────────────────────────────
    /// <summary>
    /// ดึง deck state ปัจจุบัน (สำหรับ save game)
    /// </summary>
    public DeckState GetDeckState() {
        return new DeckState {
            DrawPile = _drawPile.Select(c => c.name).ToList(),
            DiscardPile = _discardPile.Select(c => c.name).ToList()
        };
    }

    /// <summary>
    /// โหลด deck state (สำหรับ load game)
    /// </summary>
    public void LoadDeckState(DeckState state) {
        // TODO: implement load deck from saved names
        Debug.LogWarning("LoadDeckState not yet implemented");
    }
}

// ═══════════════════════════════════════════════════════════════
// Data Structures
// ═══════════════════════════════════════════════════════════════

[System.Serializable]
public class DeckState {
    public List<string> DrawPile;
    public List<string> DiscardPile;
}
