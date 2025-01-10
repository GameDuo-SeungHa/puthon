using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace puthon.Socket.Messages;

public abstract class NetworkMessageHandler
{
    public ref struct Registration
    {
        public Registration Add<THandler>()
            where THandler : NetworkMessageHandler, new()
        {
            THandler e = new();
            e.Initialize();
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

    protected internal abstract void Initialize();
    
    public abstract void Process(in IClient client, ArraySegment<byte> value);
    public abstract void Process(in IClient client, JObject jo);
}

public abstract class NetworkMessageHandler<TMessage> : NetworkMessageHandler
    where TMessage : unmanaged, INetworkMessage
{
    private static Func<TMessage, MessageWrapper<TMessage>> s_WrapperFunc;

    public static MessageWrapper<TMessage> CreateMessage(TMessage msg)
    {
        return s_WrapperFunc.Invoke(msg);
    }
    
    protected NetworkMessageHandler()
    {
        s_WrapperFunc = x =>
        {
            return new MessageWrapper<TMessage>(MessageType, x);
        };
    }

    protected internal override void Initialize()
    {
    }
    
    public sealed override void Process(in IClient client, ArraySegment<byte> value)
    {
        var e = MemoryMarshal.Cast<byte, TMessage>(value);

        TMessage msg = e[0];
        Process(client, msg);
    }
    public sealed override void Process(in IClient client, JObject jo)
    {
        TMessage msg = jo.ToObject<TMessage>();
        Process(client, msg);
    }

    public abstract void Process(in IClient client, in TMessage message);
}