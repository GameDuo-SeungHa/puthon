using JetBrains.Annotations;
using puthon.Socket.Messages;

namespace puthon.Socket;

[PublicAPI]
public interface IClient
{
    ISettings Settings { get; }

    void Send<TMessage>(TMessage message) where TMessage : unmanaged, INetworkMessage;
}