using Microsoft.AspNetCore.SignalR;
using SpinBladeArena.Hubs;
using SpinBladeArena.Performance;

namespace SpinBladeArena.LogicCenter.Push;

public class PushManager(int lobbyId, IHubContext<GameHub, IGameHubClient> hub, int serverFps)
{
    const float AllowedDelaySeconds = 2;

    private readonly CircularList<PushState> _states = new((int)Math.Ceiling(serverFps * AllowedDelaySeconds));

    public PushState Latest => _states.LastOrDefault() ?? new PushState(0, [], [], []);

    public void Push(PushState state)
    {
        _states.Add(state);
        hub.Clients.Group(lobbyId.ToString()).Update(state);
    }
}