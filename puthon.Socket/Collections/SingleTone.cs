using JetBrains.Annotations;

namespace puthon.Socket.Collections;

[PublicAPI]
public abstract class SingleTone<T>
    where T : new()
{
    private static T m_Instance;

    public static T Instance
    {
        get
        {
            if (m_Instance is null)
            {
                m_Instance = new T();
            }
            return m_Instance;
        }
    }
}