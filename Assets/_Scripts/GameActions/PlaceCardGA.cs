using System.Collections.Generic;
using UnityEngine;

public class PlaceCardGA : GameAction {
    public CardView Card;
    public Vector2Int Position;

    public PlaceCardGA(CardView card, Vector2Int pos) {
        Card = card;
        Position = pos;
    }
}