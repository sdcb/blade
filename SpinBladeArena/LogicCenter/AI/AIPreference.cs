namespace SpinBladeArena.LogicCenter.AI;

public enum AIPreference
{
    // never attack/chase, just grow and dodge, prefer more health
    Peaceful,

    // attack/chase whenever possible, prefer more blades first
    Aggressive,

    // not attack/chase if not strong enough, prefer blade length/damage first
    Defensive
}
