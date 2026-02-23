using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // จำเป็นต้องใช้สำหรับ Drag & Drop

public class Slot {
    public SlotType SlotType { get; private set; }
    public Vector2Int Position { get; private set; }
    public bool IsOccupied => OccupyingCard != null;
    public Card OccupyingCard { get; private set; }

    public Slot(SlotType slotType, Vector2Int position) {
        SlotType = slotType;
        Position = position;
    }

    public void PlaceCard(Card card) {
        OccupyingCard = card;
    }

    public void ClearCard() {
        OccupyingCard = null;
    }
}