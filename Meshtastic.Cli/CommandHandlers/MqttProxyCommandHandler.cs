using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Display;
using Meshtastic.Protobufs;

namespace Meshtastic.Cli.CommandHandlers;

public class MqttProxyCommandHandler : DeviceCommandHandler
{
    public MqttProxyCommandHandler(DeviceConnectionContext context, CommandContext commandContext) : base(context, commandContext) { }

    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        await Connection.ReadFromRadio((fromRadio, container) =>
        {
            if (fromRadio?.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.MqttClientProxyMessage &&
                fromRadio.MqttClientProxyMessage is not null)
            {
                var message = fromRadio.MqttClientProxyMessage;
                var topic = message.Topic;
                AnsiConsole.MarkupLine($"[green]Topic:[/] {topic}");
                AnsiConsole.MarkupLine($"[green]Text:[/] {message.Text}");
                AnsiConsole.MarkupLine($"[green]Data:[/] {ServiceEnvelope.Parser.ParseFrom(message.Data)}");
            }
            return Task.FromResult(false);
        });
    }
}
