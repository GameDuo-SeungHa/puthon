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

                if (res.MessageType is WebSocketMessageType.Text)
                {
                    ProcessTextData(data);
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
            throw;
        }
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

                await SendMessageAsync("test", CancellationToken.None);

                Thread.Sleep(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        finally
        {
            
        }
    }

    private void ProcessTextData(ArraySegment<byte> data)
    {
        string text = Encoding.UTF8.GetString(data);
       
    }
    private void ProcessJsonData(ArraySegment<byte> data)
    {
        string text = Encoding.UTF8.GetString(data);
        JObject jo;
        try
        {
            jo = JObject.Parse(text);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return;
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
                handler.Process(value);
            }
            else
            {
                switch (wrapper->messageType)
                {
                    case MessageType.Text:
                        ProcessTextData(value);
                        break;
                    case MessageType.Json:
                        ProcessJsonData(value);
                        break;
                    default:
                        throw new NotImplementedException();
                }
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