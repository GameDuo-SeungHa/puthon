using System.Diagnostics;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using puthon.Socket.Collections;
using puthon.Socket.Messages;

namespace puthon.Socket;

internal sealed class WebSocketClient : IClient, IDisposable
{
    private static ulong s_ConnectionCount;

    private static readonly Pool<WebSocketClient> s_Pool = new(() => new(), null, null);
    private static readonly Dictionary<ulong, IClient> s_ClientMap = new();

    public static IReadOnlyDictionary<ulong, IClient> Clients => s_ClientMap;
    
    public static WebSocketClient Create(WebSocket socket)
    {
        WebSocketClient e = s_Pool.GetOrCreate();
        e.Initialize(socket);
        s_ClientMap[s_ConnectionCount++] = e;

        return e;
    }
    
    private WebSocket? m_Socket;
    private CancellationTokenSource? m_Cts;
    private CancellationToken m_CancellationToken;

    public bool Disposed { get; private set; }
    
    private void Initialize(WebSocket ws)
    {
        Disposed = false;
        m_Socket = ws;
    }
    public void Dispose()
    {
        ThrowIfDisposed();
        
        m_Cts?.Cancel();
        m_Cts?.Dispose();
        m_Cts = null;
        
        Disposed = true;
        s_Pool.Reserve(this);
    }

    [Conditional("DEBUG")]
    private void ThrowIfDisposed()
    {
        if (!Disposed) return;

        throw new ObjectDisposedException(nameof(WebSocketClient));
    }

    public Task StartAsync()
    {
        ThrowIfDisposed();
        
        m_Cts = new();
        m_CancellationToken = m_Cts.Token;
        
        Task
            t0 = Task.Run(UpdateReceiveAsync, m_CancellationToken),
            t1 = Task.Run(UpdateSendAsync, m_CancellationToken);
        
        return Task.WhenAll(t0, t1);
    }

    private async Task UpdateReceiveAsync()
    {
        try
        {
            var buffer = new byte[1024 * 4];

            while (m_Socket is not null)
            {
                if (m_Socket.State is WebSocketState.Closed
                    or WebSocketState.Aborted)
                {
                    break;
                }

                var res = await m_Socket.ReceiveAsync(buffer, CancellationToken.None);

                if (res.MessageType is WebSocketMessageType.Close)
                {
                    break;
                }

                ArraySegment<byte> data = new ArraySegment<byte>(
                    buffer, 0, res.Count);

                Console.WriteLine($"Message received type: {res.MessageType}");

                if (res.MessageType is WebSocketMessageType.Text)
                {
                    string text = Encoding.UTF8.GetString(data);
                    Console.WriteLine(text);
                    JObject jo;
                    try
                    {
                        jo = JObject.Parse(text);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        continue;
                    }

                    MessageType msgType = (MessageType)(jo["messageType"] ?? 0).Value<ulong>();

                    if (msgType is MessageType.Undefined)
                    {
                        Console.WriteLine("msg type is not provided");
                        continue;
                    }

                    if (!NetworkMessageHandler.Handlers.TryGetValue(msgType, out var handler))
                    {
                        Console.WriteLine($"msg type is not handled {msgType}");
                        continue;
                    }

                    handler.Process(this, jo);
                }
                else if (res.MessageType is WebSocketMessageType.Binary)
                {
                    ProcessBinaryData(data);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Dispose();
            throw;
        }
        
        Dispose();
    }
    private async Task UpdateSendAsync()
    {
        try
        {
            while (m_Socket is not null)
            {
                if (m_Socket.State is WebSocketState.Closed
                    or WebSocketState.Aborted)
                {
                    break;
                }

                // await SendMessageAsync("test", CancellationToken.None);

                await Task.Yield();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
   
    private unsafe void ProcessBinaryData(ArraySegment<byte> data)
    {
        if (data.Array is null)
        {
            throw new InvalidOperationException();
        }
        
        fixed (byte* ptr = &data.Array[data.Offset])
        {
            MessageWrapper* wrapper = (MessageWrapper*)ptr;
            ArraySegment<byte> value = data[(Marshal.SizeOf<MessageWrapper>())..];

            if (NetworkMessageHandler.Handlers.TryGetValue(wrapper->messageType, out var handler))
            {
                handler.Process(this, value);
            }
            else
            {
                Console.WriteLine(
                    $"Message {wrapper->messageType} is not handled");
            }
        }
    }
    
    public Task SendMessageAsync(
        string msg, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        
        var bytes = Encoding.UTF8.GetBytes(msg);
        var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
        
        return SendAsync(
            WebSocketMessageType.Text,
            arraySegment,
            cancellationToken);
    }
    public Task SendAsync(WebSocketMessageType messageType, ArraySegment<byte> data, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        if (m_Socket is null)
        {
            throw new ObjectDisposedException(nameof(WebSocketClient));
        }
        
        return m_Socket.SendAsync(data,
            messageType,
            true,
            cancellationToken);
        ;
    }
}

[PublicAPI]
public interface IClient
{
    // Task SendMessageAsync(
    //     string msg, CancellationToken cancellationToken);
    // Task SendAsync(
    //     WebSocketMessageType messageType,
    //     ArraySegment<byte> data,
    //     CancellationToken cancellationToken);
}