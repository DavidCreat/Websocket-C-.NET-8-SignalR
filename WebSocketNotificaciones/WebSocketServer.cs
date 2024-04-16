using System.Net.WebSockets;
using System.Net;
using System.Text;

public class WebSocketServer
{
    private HttpListener _listener;

    public WebSocketServer(string url)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(url);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine("WebSocket server started.");

        while (true)
        {
            var context = await _listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                HandleWebSocketAsync(webSocketContext.WebSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    private async Task HandleWebSocketAsync(WebSocket socket)
    {
        try
        {
            // Envía una notificación cuando un cliente se conecta
            var newClientMessage = "¡Nuevo usuario en el grupo!";
            var buffer = Encoding.UTF8.GetBytes(newClientMessage);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            // Resto del manejo del WebSocket...
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en el manejo del WebSocket: {ex}");
        }
        finally
        {
            if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseSent)
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Fin de la conexión", CancellationToken.None);
            socket.Dispose();
        }
    }
}
