using System.Net.WebSockets;
using System.Text;
using puthon.Socket.Collections;

namespace puthon.Socket;

internal sealed class Client : IClient, IDisposable
{
    private static ulong s_ConnectionCount;

    private static readonly Pool<Client> s_Pool = new();
    private static readonly Dictionary<ulong, Client> s_ClientMap = new();

    public static IReadOnlyDictionary<ulong, Client> Clients => s_ClientMap;
    
    public static Client Create(WebSocket socket)
    {
        Client e = s_Pool.GetOrCreate();
        e.Initialize(socket);
        s_ClientMap[s_ConnectionCount++] = e;

        return e;
    }
    
    private WebSocket m_Socket;

    private void Initialize(WebSocket ws)
    {
        m_Socket = ws;
    }
    public void Dispose()
    {
        s_Pool.Reserve(this);
    }

    public Task StartAsync()
    {
        return Task.Run(async () => await Update());
    }
    
    private async Task Update()
    {
        try
        {
            while (true)
            {
                if (m_Socket.State is WebSocketState.Closed
                    or WebSocketState.Aborted)
                {
                    break;
                }

                await SendMessage(m_Socket, "test");

                Thread.Sleep(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            
        }
    }
    private static Task SendMessage(WebSocket t, string msg)
    {
        var bytes = Encoding.UTF8.GetBytes(msg);
        var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
        
        return t.SendAsync(arraySegment,
            WebSocketMessageType.Text,
            false,
            CancellationToken.None);
        ;
    }
}

public interface IClient
{
    
}