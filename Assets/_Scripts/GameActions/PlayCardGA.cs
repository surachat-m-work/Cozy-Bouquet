using System.Collections.Generic;
using UnityEngine;

public class PlayCardGA : GameAction {
    [SerializeField] private CardView _card;
    [SerializeField] private List<SlotView> _slots;

    public CardView Card => _card;
    public List<SlotView> Slots => _slots;

    public PlayCardGA(CardView card, List<SlotView> slots) {
        _card = card;
        _slots = slots;
    }
}