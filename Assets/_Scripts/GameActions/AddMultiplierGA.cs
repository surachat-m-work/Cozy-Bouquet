public class AddMultiplierGA : GameAction 
{
    public float Multiplier { get; private set; }
    public AddMultiplierGA(float multiplier) => Multiplier = multiplier;
}