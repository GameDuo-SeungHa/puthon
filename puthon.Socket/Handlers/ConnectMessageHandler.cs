using puthon.Socket.Messages.System;

namespace puthon.Socket.Messages.Handlers;

public sealed class ConnectMessageHandler : NetworkMessageHandler<ConnectMessage>
{
    public override MessageType MessageType => MessageType.Connect;


    public override void Process(in IClient client, in ConnectMessage message)
    {
        Console.WriteLine("connect msg received");
    }
}