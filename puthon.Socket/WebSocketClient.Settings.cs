namespace puthon.Socket;

partial class WebSocketClient
{
    private readonly Settings m_Settings = new();

    public ISettings Settings => m_Settings;

    private partial void DisposeSettings()
    {
        m_Settings.Clear();
    }
}