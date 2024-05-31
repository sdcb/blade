using SpinBladeArena.LogicCenter.Push;

namespace SpinBladeArena.LogicCenter;

public record StatInfo
{
    public int Score { get; set; } = 1;
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int DestroyBlades { get; set; }

    public StatInfo CopyResetScore()
    {
        return this with { Score = 1 };
    }

    public StatInfoDto ToDto(int userId) => new()
    {
        UserId = userId,
        Score = Score,
        Kills = Kills,
        Deaths = Deaths,
        DestroyBlades = DestroyBlades,
    };
}
