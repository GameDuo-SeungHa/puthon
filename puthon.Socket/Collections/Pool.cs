using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace puthon.Socket.Collections;

[PublicAPI]
internal sealed class Pool<TObject>
    where TObject : new()
{
    private readonly Stack<TObject> m_Stack = new();
    private readonly HashSet<int> m_HashSet = new();

    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public TObject GetOrCreate()
    {
        if (m_Stack.TryPop(out var e) || e is null)
        {
            e = new();
        }
        m_HashSet.Add(e.GetHashCode());
        return e;
    }
    public void Reserve([DisallowNull] TObject o)
    {
        m_Stack.Push(o);
    }
}