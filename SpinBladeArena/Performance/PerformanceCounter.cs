using System.Diagnostics;

namespace SpinBladeArena.Performance;

public class PerformanceCounter(Stopwatch Stopwatch)
{
    public static PerformanceCounter Start() => new(Stopwatch.StartNew());

    public TimeSpan Sleep { get; private set; }

    public TimeSpan AddPlayerRequest { get; private set; }

    public TimeSpan Move { get; private set; }

    public TimeSpan Bonus { get; private set; }

    public TimeSpan Attack { get; private set; }

    public TimeSpan Dead { get; private set; }

    public TimeSpan BonusSpawn { get; private set; }

    public TimeSpan PlayerSpawn { get; private set; }

    public TimeSpan DispatchMessage { get; private set; }

    public void RecordSleep()
    {
        Sleep = Stopwatch.Elapsed;
        Stopwatch.Restart();
    }

    public void RecordAddPlayerRequests()
    {
        AddPlayerRequest = Stopwatch.Elapsed;
        Stopwatch.Restart();
    }

    public void RecordMove()
    {
        Move = Stopwatch.Elapsed;
        Stopwatch.Restart();
    }

    public void RecordBonus()
    {
        Bonus = Stopwatch.Elapsed;
        Stopwatch.Restart();
    }

    public void RecordAttack()
    {
        Attack = Stopwatch.Elapsed;
        Stopwatch.Restart();
    }

    public void RecordDead()
    {
        Dead = Stopwatch.Elapsed;
        Stopwatch.Restart();
    }

    public void RecordBonusSpawn()
    {
        BonusSpawn = Stopwatch.Elapsed;
        Stopwatch.Restart();
    }

    public void RecordPlayerSpawn()
    {
        PlayerSpawn = Stopwatch.Elapsed;
        Stopwatch.Restart();
    }

    public void RecordDispatchMessage()
    {
        DispatchMessage = Stopwatch.Elapsed;
        Stopwatch.Restart();
    }

    public PerformanceData ToPerformanceData()
    {
        return new PerformanceData(
            Sleep,
            AddPlayerRequest,
            Move,
            Bonus,
            Attack,
            Dead,
            BonusSpawn,
            PlayerSpawn,
            DispatchMessage
        );
    }
}
