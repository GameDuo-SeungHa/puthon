// See https://aka.ms/new-console-template for more information

using System.Net.WebSockets;
using System.Text;

Console.WriteLine("Hello, World!");

using var ws = new ClientWebSocket();
Console.WriteLine("connect");

ws.Options.SetBuffer(1024 * 4, 1024 * 4);
await ws.ConnectAsync(new Uri("ws://localhost:5214/ws"), CancellationToken.None);

Console.WriteLine("connect success");

await Task.Run(async () =>
{
    var buffer = new byte[1024 * 4];
    
    while (true)
    {
        if (ws.State is WebSocketState.Closed or WebSocketState.Aborted)
        {
            break;
        }
        
        var res = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (res.MessageType is WebSocketMessageType.Close)
        {
            break;
        }
        
        var msg = Encoding.UTF8.GetString(buffer, 0, res.Count);
        Console.WriteLine($"msg: {msg}");
    }
});
