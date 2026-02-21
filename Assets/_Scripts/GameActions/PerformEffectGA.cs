using UnityEngine;

public class PerformEffectGA : GameAction {
    [SerializeField] private Effect _effect;
    [SerializeField] private CardView _card;

    public Effect Effect => _effect;
    public CardView Card => _card;

    public PerformEffectGA(Effect effect, CardView card) {
        _effect = effect;
        _card = card;
    }
}