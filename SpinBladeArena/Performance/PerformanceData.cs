namespace SpinBladeArena.Performance;

public record PerformanceData(
    TimeSpan Sleep,
    TimeSpan AddPlayerRequest,
    TimeSpan Move,
    TimeSpan Bonus,
    TimeSpan Attack,
    TimeSpan Dead,
    TimeSpan BonusSpawn,
    TimeSpan PlayerSpawn,
    TimeSpan DispatchMessage
)
{
    public TimeSpan AllExceptSleep => AddPlayerRequest + Move + Bonus + Attack + Dead + BonusSpawn + PlayerSpawn + DispatchMessage;

    public static PerformanceData Zero { get; } = new(
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero);

    public static PerformanceData operator +(PerformanceData a, PerformanceData b) => new(
        a.Sleep + b.Sleep,
        a.AddPlayerRequest + b.AddPlayerRequest,
        a.Move + b.Move,
        a.Bonus + b.Bonus,
        a.Attack + b.Attack,
        a.Dead + b.Dead,
        a.BonusSpawn + b.BonusSpawn,
        a.PlayerSpawn + b.PlayerSpawn,
        a.DispatchMessage + b.DispatchMessage);

    public static PerformanceData operator /(PerformanceData a, int b) => new(
        TimeSpan.FromTicks(a.Sleep.Ticks / b),
        TimeSpan.FromTicks(a.AddPlayerRequest.Ticks / b),
        TimeSpan.FromTicks(a.Move.Ticks / b),
        TimeSpan.FromTicks(a.Bonus.Ticks / b),
        TimeSpan.FromTicks(a.Attack.Ticks / b),
        TimeSpan.FromTicks(a.Dead.Ticks / b),
        TimeSpan.FromTicks(a.BonusSpawn.Ticks / b),
        TimeSpan.FromTicks(a.PlayerSpawn.Ticks / b),
        TimeSpan.FromTicks(a.DispatchMessage.Ticks / b));
}