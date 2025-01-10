using System.Runtime.InteropServices;

namespace puthon.Socket.Collections;

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct ValueUnion32
{
    [FieldOffset(0)] public sbyte sbyteValue;
    [FieldOffset(0)] public byte byteValue;
    [FieldOffset(0)] public bool boolValue;
    [FieldOffset(0)] public int intValue;
    [FieldOffset(0)] public uint uintValue;
    [FieldOffset(0)] public float floatValue;

    public static implicit operator ValueUnion32(bool v) => new() { boolValue = v };
    public static implicit operator bool(ValueUnion32 v) => v.boolValue;
}