namespace SpinBladeArena.LogicCenter;

public class GameManager
{
    public List<Lobby> Lobbies { get; } = [];

    public List<User> Users { get; } = [];

    public static GameManager Instance { get; } = new();
}

public record User(string Name);