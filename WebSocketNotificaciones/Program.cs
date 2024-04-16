using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static List<string> userList = new List<string>(); // Lista de nombres de usuarios

    static async Task Main(string[] args)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("Servidor WebSocket iniciado.");

        while (true)
        {
            var context = await listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                var wsContext = await context.AcceptWebSocketAsync(null);
                _ = HandleWebSocket(wsContext.WebSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    static async Task HandleWebSocket(WebSocket socket)
    {
        try
        {
            var receiveBuffer = new byte[1024];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                    userList.Add(receivedMessage); // Agregar el nombre a la lista de usuarios
                    Console.WriteLine($"{receivedMessage} se unió al grupo"); // Mostrar mensaje en la consola
                    await SendUserList(socket); // Enviar la lista actualizada a todos los clientes
                }
            }
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

    static async Task SendUserList(WebSocket socket)
    {
        var userListJson = JsonSerializer.Serialize(userList);
        var buffer = Encoding.UTF8.GetBytes(userListJson);
        await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
