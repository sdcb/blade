namespace SpinBladeArena.LogicCenter.Lobbies;

public sealed class FFALobby(int id, int createUserId, DateTime createTime, IServiceProvider ServiceProvider) : Lobby(id, createUserId, createTime, ServiceProvider)
{
}