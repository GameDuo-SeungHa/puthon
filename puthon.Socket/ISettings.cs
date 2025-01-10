using JetBrains.Annotations;
using puthon.Socket.Collections;

namespace puthon.Socket;

[PublicAPI]
public interface ISettings
{
    ValueUnion32 GetValue(SettingType type);
    void SetValue(SettingType type, ValueUnion32 value);
}

public enum SettingType : sbyte
{
    PreferJson
}

internal sealed class Settings : Dictionary<SettingType, ValueUnion32>, ISettings
{
    public ValueUnion32 GetValue(SettingType type)
    {
        return this[type];
    }
    public void SetValue(SettingType type, ValueUnion32 value)
    {
        this[type] = value;
    }
}