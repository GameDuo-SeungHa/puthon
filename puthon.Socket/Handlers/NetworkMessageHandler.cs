using System.Runtime.InteropServices;

namespace puthon.Socket.Messages;

public abstract class NetworkMessageHandler
{
    public ref struct Registration
    {
        public Registration Add<THandler>()
            where THandler : NetworkMessageHandler, new()
        {
            THandler e = new();
            s_Handlers.Add(e.MessageType, e);
            return this;
        }
    }
    
    public static Registration Create<THandler>()
        where THandler : NetworkMessageHandler, new()
    {
        Registration e = new Registration();
        e.Add<THandler>();
        return e;
    }
    
    private static readonly Dictionary<MessageType, NetworkMessageHandler> s_Handlers = new();

    public static IReadOnlyDictionary<MessageType, NetworkMessageHandler> Handlers => s_Handlers;

    public abstract MessageType MessageType { get; }
    
    public abstract void Process(ArraySegment<byte> value);
}

public abstract class NetworkMessageHandler<TMessage> : NetworkMessageHandler
    where TMessage : unmanaged, INetworkMessage
{
    public sealed override void Process(ArraySegment<byte> value)
    {
        var e = MemoryMarshal.Cast<byte, TMessage>(value);

        TMessage msg = e[0];
        OnProcess(msg);
    }

    protected abstract void OnProcess(in TMessage message);
}