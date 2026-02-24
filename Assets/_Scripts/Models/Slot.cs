using UnityEngine;

public class Slot {
    public SlotType SlotType { get; private set; }
    public Vector2Int Position { get; private set; }
    public bool IsOccupied => OccupyingCard != null;
    public CardView OccupyingCard { get; private set; }

    public Slot(SlotType slotType, Vector2Int position) {
        SlotType = slotType;
        Position = position;
    }

    public void PlaceCard(CardView card) {
        OccupyingCard = card;
    }

    public void ClearCard() {
        OccupyingCard = null;
    }
}