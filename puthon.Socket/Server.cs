using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace puthon.Socket;

using System.Net;
using System.Net.Sockets;

// https://medium.com/bina-nusantara-it-division/implementing-websocket-client-and-server-on-asp-net-core-6-0-c-4fbda11dbceb
// https://medium.com/@Alikhalili/building-a-high-performance-tcp-server-from-scratch-a8ede35c4cc2

[PublicAPI]
public sealed class Server : IDisposable
{
    public static async Task WebSocketHandler(HttpContext ctx)
    {
        if (!ctx.WebSockets.IsWebSocketRequest)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        
        var ws = await ctx.WebSockets.AcceptWebSocketAsync();
        // var task = Task.Run(() => HandleConnection(ws));
        
        // var message = "Hello World!";
        // var bytes = Encoding.UTF8.GetBytes(message);
        // var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
        //
        // while (true)
        // {
        //     if (ws.State is WebSocketState.Closed
        //         or WebSocketState.Aborted)
        //     {
        //         break;
        //     }
        //
        //     if (ws.State is WebSocketState.Open)
        //     {
        //         Console.WriteLine("send msg");
        //         await ws.SendAsync(arraySegment,
        //             WebSocketMessageType.Text,
        //             true,
        //             CancellationToken.None);
        //             ;
        //     }
        //         
        //     Thread.Sleep(1000);
        // }

        var t = Task.Run(() => TestThread(ws));
        await t;
        
        // var handler = new ConnectionHandler(ws);
        // ThreadPool.UnsafeQueueUserWorkItem(handler, preferLocal: false);
        
        Console.WriteLine("Test success");
    }

    private static void TestThread(object? ctx)
    {
        try
        {
            var ws = (WebSocket)ctx;

            while (true)
            {
                if (ws.State is WebSocketState.Closed
                    or WebSocketState.Aborted)
                {
                    break;
                }

                SendMessage(ws, "test");
                
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
            true,
            CancellationToken.None);
        ;
    }
    private static void HandleConnection(WebSocket ws)
    {
        var handler = new ConnectionHandler(ws);
        handler.Execute();
    }
    
    private readonly Socket m_Listener;

    private Thread? m_UpdateThread;

    public bool IsRunning { get; private set; }
    
    // public Server(int port)
    // {
    //     m_Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    //     m_Listener.Bind(new IPEndPoint(IPAddress.Any, port));
    // }
    public void Dispose()
    {
        m_UpdateThread?.Interrupt();
        m_Listener.Dispose();
    }
    //
    // public void Start()
    // {
    //     Assert.That(!IsRunning, "is already running");
    //     
    //     m_Listener.Listen(backlog: 4096);
    //     IsRunning = true;
    //
    //     m_UpdateThread = new Thread(Update);
    //     m_UpdateThread.Start();
    // }
    //
    // private void Update()
    // {
    //     while (true)
    //     {
    //         var acceptSocket = m_Listener.Accept();
    //         var handler = new ConnectionHandler(acceptSocket);
    //         ThreadPool.UnsafeQueueUserWorkItem(handler, preferLocal: false);
    //     }
    // }
    
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


public sealed class Client
{
    
}