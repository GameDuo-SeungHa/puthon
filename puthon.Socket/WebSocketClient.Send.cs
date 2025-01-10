using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using puthon.Socket.Messages;

namespace puthon.Socket;

internal partial class WebSocketClient
{
    private partial async Task UpdateSendAsync()
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
    
    private Task SendMessageAsync(
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
    private Task SendAsync(WebSocketMessageType messageType, ArraySegment<byte> data, CancellationToken cancellationToken)
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

    private readonly ConcurrentQueue<Buffer> m_SendQueue = new();
    
    unsafe void IClient.Send<TMessage>(TMessage message)
    {
        MessageWrapper<TMessage> wrapper = NetworkMessageHandler<TMessage>.CreateMessage(message);

        if (Settings.GetValue(SettingType.PreferJson).boolValue)
        {
            string json = JsonConvert.SerializeObject(wrapper, Formatting.None);
            Buffer buffer = Buffer.GetOrCreate();
            buffer.Set(json);

            m_SendQueue.Enqueue(buffer);
        }
        else
        {
            
        }
        TMessage* ptr = &message;
        
        
    }
}