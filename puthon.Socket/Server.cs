using System.Net.WebSockets;
using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using puthon.Socket.Messages;
using System.Net;
using puthon.Socket.Messages.Handlers;

namespace puthon.Socket;

// https://medium.com/bina-nusantara-it-division/implementing-websocket-client-and-server-on-asp-net-core-6-0-c-4fbda11dbceb
// https://medium.com/@Alikhalili/building-a-high-performance-tcp-server-from-scratch-a8ede35c4cc2

[PublicAPI]
public static class Server
{
    public static void Initialize()
    {
        NetworkMessageHandler
            .Create<ConnectMessageHandler>()
            ;
    }
    
    public static async Task WebSocketHandler(HttpContext ctx)
    {
        if (!ctx.WebSockets.IsWebSocketRequest)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        
        var ws = await ctx.WebSockets.AcceptWebSocketAsync();
        await WebSocketClient
                .Create(ws)
                .StartAsync()
            ;

        // await Task.Run(async () => await TestThread(ws));
        
        Console.WriteLine("Test success");
    }
    
    class ConnectionHandler : IThreadPoolWorkItem
    {
        private readonly WebSocket m_AcceptSocket;
        
        public ConnectionHandler(WebSocket mAcceptSocket)
        {
            m_AcceptSocket = mAcceptSocket;
        }
  
        public void Execute()
        {
            Console.WriteLine("connection start");
            
            var message = "Hello World!";
            var bytes = Encoding.UTF8.GetBytes(message);
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);

            while (true)
            {
                if (m_AcceptSocket.State is WebSocketState.Closed
                    or WebSocketState.Aborted)
                {
                    break;
                }

                if (m_AcceptSocket.State is WebSocketState.Open)
                {
                    Console.WriteLine("send msg");
                    m_AcceptSocket.SendAsync(arraySegment,
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None)
                        .GetAwaiter().GetResult()
                        ;
                }
                
                Thread.Sleep(1000);
            }
        }
    }
    
    // class SocketAsyncEngine : IThreadPoolWorkItem 
    // {
    //     ConcurrentQueue<SocketIOEvent> eventQueue = _eventQueue;
    //
    //     public SocketAsyncEngine()
    //     {
    //         var thread = new Thread(static s => ((SocketAsyncEngine)s!).EventLoop())
    //         {
    //             IsBackground = true,
    //             Name = ".NET Sockets"
    //         };
    //         thread.UnsafeStart(this);
    //     }
    //
    //     private void EventLoop() 
    //     {
    //         SocketEventHandler handler = new SocketEventHandler(this);
    //         while (true)
    //         {
    //             Interop.Sys.WaitForSocketEvents(_port, handler.Buffer, &numEvents);
    //             if (handler.HandleSocketEvents(numEvents))
    //             {
    //                 ThreadPool.UnsafeQueueUserWorkItem(this, preferLocal: false);
    //             }
    //         }
    //     }
    //
    //     public void Execute()
    //     {
    //         while (true)
    //         {
    //             if (eventQueue.TryDequeue(out ev))
    //             {
    //                 break;
    //             }
    //             ev.Context.HandleEvents(ev.Events);
    //         }
    //     }
    // }
}