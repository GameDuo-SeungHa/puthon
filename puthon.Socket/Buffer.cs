using System.Text;
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
    private int m_Length = 0;

    public ArraySegment<byte> Value => new(m_Data, 0, m_Length);
    
    public void Set(string str)
    {
        if (str.Length > m_Data.Length)
        {
            Array.Resize(ref m_Data, m_Data.Length * 2);
        }
        
        var ptr = str.AsSpan();
        var buffer = m_Data.AsSpan();
        m_Length = Encoding.UTF8.GetBytes(ptr, buffer);
    }

    public void Dispose()
    {
        s_Pool.Reserve(this);
    }
}