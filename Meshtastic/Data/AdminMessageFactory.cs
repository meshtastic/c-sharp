using Meshtastic.Protobufs;

namespace Meshtastic.Data;

public class AdminMessageFactory 
{
    private readonly DeviceStateContainer container;

    public AdminMessageFactory(DeviceStateContainer container)
    {
        this.container = container;
    }

    public AdminMessage CreateBeginEditSettingsMessage() =>
        new()
        {
            BeginEditSettings = true
        };

    public AdminMessage CreateCommitEditSettingsMessage() =>
        new()
        {
            CommitEditSettings = true
        };
}