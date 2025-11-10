using Microsoft.Extensions.Logging;

namespace Brimborium.Tracerit.Server;


public class WebSocketHandler {
    private readonly WebSocketConnectionManager _ConnectionManager;
    private readonly ILogger<WebSocketHandler> _Logger;

    public WebSocketHandler(
        WebSocketConnectionManager connectionManager,
        ILogger<WebSocketHandler> logger) {
        this._ConnectionManager = connectionManager;
        this._Logger = logger;
    }

    public async Task HandleAsync(HttpContext context, WebSocket webSocket) {
        var connectionId = this._ConnectionManager.AddConnection(webSocket);
        this._Logger.LogInformation($"WebSocket connection established: {connectionId}");

        try {
            await this.ReceiveAsync(connectionId, webSocket);
        } catch (Exception ex) {
            this._Logger.LogError(ex, $"Error in WebSocket connection {connectionId}");
        } finally {
            this._ConnectionManager.RemoveConnection(connectionId);
            this._Logger.LogInformation($"WebSocket connection closed: {connectionId}");
        }
    }

    private async Task ReceiveAsync(string connectionId, WebSocket webSocket) {
        
        var bufferBytes = new byte[4096];
        var buffer = new ArraySegment<byte>(bufferBytes);

        while (webSocket.State == WebSocketState.Open) {
            var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

            switch (result.MessageType) {
                case WebSocketMessageType.Text:
                    //var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    //await this.HandleTextMessage(connectionId, message);
                    //break;
                    throw new NotImplementedException();

                case WebSocketMessageType.Binary:
                    //await this.HandleBinaryMessage(connectionId, buffer.Array.Take(result.Count).ToArray());
                    //break;
                    throw new NotImplementedException();

                case WebSocketMessageType.Close:
                    await webSocket.CloseAsync(
                        result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                        result.CloseStatusDescription,
                        CancellationToken.None
                    );
                    break;
            }
        }
    }

    private async Task HandleTextMessage(string connectionId, string message) {
        this._Logger.LogInformation($"Received from {connectionId}: {message}");

        try {
            var json = JsonDocument.Parse(message);
            var type = json.RootElement.GetProperty("type").GetString();

            switch (type) {
                case "broadcast":
                    await this.BroadcastMessage(connectionId, message);
                    break;
                case "private":
                    await SendPrivateMessage(connectionId, json.RootElement);
                    break;
                default:
                    await this.Echo(connectionId, message);
                    break;
            }
        } catch (JsonException) {
            await this.Echo(connectionId, message);
        }
    }

    private async Task HandleBinaryMessage(string connectionId, byte[] data) {
        this._Logger.LogInformation($"Received binary from {connectionId}: {data.Length} bytes");
        // Process binary data
    }

    private async Task Echo(string connectionId, string message) {
        await this._ConnectionManager.SendAsync(connectionId, $"Echo: {message}");
    }

    private async Task BroadcastMessage(string senderId, string message) {
        await this._ConnectionManager.BroadcastAsync(message, senderId);
    }

    private async Task SendPrivateMessage(string senderId, JsonElement json) {
        var targetId = json.GetProperty("target").GetString();
        var content = json.GetProperty("content").GetString();

        await this._ConnectionManager.SendAsync(targetId, content);
    }
}