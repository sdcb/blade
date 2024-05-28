using Microsoft.AspNetCore.SignalR;
using SpinBladeArena.Hubs;
using SpinBladeArena.Performance;
using System.Text.Encodings.Web;
using System.Text.Json;

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

        // Uncomment the following line to write debug info to the file system
        // WriteDebugInfo(state);
    }

    private void WriteDebugInfo(PushState state)
    {
        Directory.CreateDirectory($"pushes/{lobbyId}");
        string jsonData = JsonSerializer.Serialize(state, _jso);
        File.WriteAllText($"pushes/{lobbyId}/{state.FrameId}.json", jsonData);
    }

    readonly static JsonSerializerOptions _jso = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
}