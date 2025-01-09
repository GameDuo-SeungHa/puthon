using puthon.Socket.Collections;

namespace puthon.Socket;

internal sealed class Buffer(int size) : IDisposable
{
    private const int DefaultSize = 1024;
    
    private static readonly Pool<Buffer> s_Pool = new(() => new(DefaultSize), null, null);

    public static Buffer GetOrCreate()
    {
        var res = s_Pool.GetOrCreate();
        return res;
    }
    
    private byte[] m_Data = new byte[size];

    public void Dispose()
    {
        s_Pool.Reserve(this);
    }
}