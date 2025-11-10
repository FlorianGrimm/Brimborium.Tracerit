namespace Brimborium.Tracerit.Server;

/*
    https://websocket.org/guides/languages/csharp/
 */

public class WebSocketConnectionManager {
    private readonly ConcurrentDictionary<string, WebSocket> _Connections = new();

    public string AddConnection(WebSocket webSocket) {
        var connectionId = Guid.NewGuid().ToString();
        this._Connections.TryAdd(connectionId, webSocket);
        return connectionId;
    }

    public void RemoveConnection(string connectionId) {
        this._Connections.TryRemove(connectionId, out _);
    }

    public WebSocket? GetConnection(string connectionId) {
        this._Connections.TryGetValue(connectionId, out var connection);
        return connection;
    }

    public IEnumerable<string> GetAllConnectionIds() {
        return this._Connections.Keys;
    }

    public async Task SendAsync(string connectionId, ArraySegment<byte> messageAsUtf8) {
        if (this._Connections.TryGetValue(connectionId, out var webSocket)) {
            if (webSocket.State == WebSocketState.Open) {
                await webSocket.SendAsync(
                    messageAsUtf8,
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
        }
    }

    public async Task SendAsync(string connectionId, string message) {
        if (this._Connections.TryGetValue(connectionId, out var webSocket)) {
            if (webSocket.State == WebSocketState.Open) {
                var bytes = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
        }
    }

    public async Task BroadcastAsync(string message, string excludeConnectionId = null) {
        var tasks = new List<Task>();

        foreach (var pair in this._Connections) {
            if (pair.Key != excludeConnectionId && pair.Value.State == WebSocketState.Open) {
                tasks.Add(this.SendAsync(pair.Key, message));
            }
        }

        await Task.WhenAll(tasks);
    }
}