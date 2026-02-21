using UnityEngine;

public class AddScoreGA : GameAction {
    [SerializeField] private int _amount;

    public int Amount => _amount;

    public AddScoreGA(int amount) {
        _amount = amount;
    }
}