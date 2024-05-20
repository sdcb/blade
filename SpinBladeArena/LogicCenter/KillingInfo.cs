namespace SpinBladeArena.LogicCenter;

public record KillingInfo(Player Player1, Player Player2, bool Player1Dead, bool Player2Dead)
{
    public bool AnyDead => Player1Dead || Player2Dead;
}