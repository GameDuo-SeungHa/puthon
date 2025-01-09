using Newtonsoft.Json;

namespace puthon.Socket.Messages.System;

[Serializable, JsonObject]
public struct ConnectMessage : INetworkMessage
{
    public bool json;
}