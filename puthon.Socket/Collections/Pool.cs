using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace puthon.Socket.Collections;

[PublicAPI]
internal sealed class Pool<TObject>(
    Func<TObject> createFunction,
    Action<TObject>? onGetFunction,
    Action<TObject>? onReserveFunction)
{
    private readonly Stack<TObject> m_Stack = new();

    [return: System.Diagnostics.CodeAnalysis.NotNull] 
    public TObject GetOrCreate()
    {
        if (m_Stack.TryPop(out var e) || e is null)
        {
            e = createFunction.Invoke();
            if (e is null)
            {
                throw new InvalidOperationException();
            }
        }

        onGetFunction?.Invoke(e);
        return e;
    }
    public void Reserve([DisallowNull] TObject o)
    {
        onReserveFunction?.Invoke(o);
        
        m_Stack.Push(o);
    }
}