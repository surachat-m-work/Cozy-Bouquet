public class AddChipsGA : GameAction 
{
    public int Amount { get; private set; }
    public AddChipsGA(int amount) => Amount = amount;
}