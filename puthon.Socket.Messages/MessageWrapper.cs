using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace puthon.Socket.Messages;

[Serializable, JsonObject(MemberSerialization.OptIn)]
public struct MessageWrapper<TMessage>(MessageType type, TMessage data)
    where TMessage : INetworkMessage
{
    [JsonProperty] public MessageType messageType = type;
    [JsonProperty] public TMessage data = data;
}

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct MessageWrapper
{
    [FieldOffset(0)]
    public MessageType messageType;
}