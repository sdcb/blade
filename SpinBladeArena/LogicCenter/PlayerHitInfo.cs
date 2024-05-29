namespace SpinBladeArena.LogicCenter;

public record PlayerHitInfo(Player Attacker, Player Defender, float Damage)
{
    public bool DefenderDead => Defender.Health <= 0;
}