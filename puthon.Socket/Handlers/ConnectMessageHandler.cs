using puthon.Socket.Messages.System;

namespace puthon.Socket.Messages.Handlers;

public sealed class ConnectMessageHandler : NetworkMessageHandler<ConnectMessage>
{
    public override MessageType MessageType { get; }
    
    protected override void OnProcess(in ConnectMessage message)
    {
        
    }
}