using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using MQTTnet;
using Meshtastic.Protobufs;
using MQTTnet.Client;
using Microsoft.Extensions.Logging;

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
        // connect to mqtt server with mqttnet
        var factory = new MqttFactory();
        using var mqttClient = factory.CreateMqttClient();
        MqttClientOptions options = GetMqttClientOptions(container);
        await mqttClient.ConnectAsync(options, CancellationToken.None);

        var root = String.IsNullOrWhiteSpace(container.LocalModuleConfig.Mqtt.Root) ? "msh" : container.LocalModuleConfig.Mqtt.Root;
        var prefix = $"{root}/{container.Metadata.FirmwareVersion.First()}";
        var subscriptionTopic = $"{prefix}/#";

        Logger.LogInformation($"Subscribing to topic: {subscriptionTopic}");
        await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
            .WithTopic(subscriptionTopic)
            .Build());

        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            if (e.ApplicationMessage.Topic.StartsWith($"{prefix}/stat/"))
                return;

            Logger.LogInformation($"Received MQTT from host on topic: {e.ApplicationMessage.Topic}");

            // Get bytes from utf8 string
            var toRadio = new ToRadioMessageFactory()
                .CreateMqttClientProxyMessage(e.ApplicationMessage.Topic, e.ApplicationMessage.PayloadSegment.ToArray(), e.ApplicationMessage.Retain);
            Logger.LogDebug(toRadio.ToString());
            await Connection.WriteToRadio(toRadio);
        };

        await Connection.ReadFromRadio(async (fromRadio, container) =>
        {
            if (fromRadio?.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.MqttClientProxyMessage &&
                fromRadio.MqttClientProxyMessage is not null)
            {
                var message = fromRadio.MqttClientProxyMessage;
                Logger.LogInformation($"Received MQTT message from device to proxy on topic: {message.Topic}");
                if (message.PayloadVariantCase == MqttClientProxyMessage.PayloadVariantOneofCase.Data)
                {
                    Logger.LogDebug(ServiceEnvelope.Parser.ParseFrom(message.Data).ToString());
                    await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                        .WithTopic(message.Topic)
                        .WithPayload(message.Data.ToByteArray())
                        .WithRetainFlag(message.Retained)
                        .Build());
                }
                else if (message.PayloadVariantCase == MqttClientProxyMessage.PayloadVariantOneofCase.Text)
                {
                    Logger.LogDebug(message.Text);
                    await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                        .WithTopic(message.Topic)
                        .WithPayload(message.Text)
                        .WithRetainFlag(message.Retained)
                        .Build());
                }
            }
            return false;
        });
    }

    private static MqttClientOptions GetMqttClientOptions(DeviceStateContainer container)
    {
        var builder = new MqttClientOptionsBuilder()
            .WithClientId(container.GetDeviceNodeInfo()?.User?.Id ?? container.MyNodeInfo.MyNodeNum.ToString());

        var address = container.LocalModuleConfig.Mqtt.Address;
        var host = address.Split(':').FirstOrDefault() ?? container.LocalModuleConfig.Mqtt.Address;
        var port = address.Contains(':') ? address.Split(':').LastOrDefault() : null;

        if (container.LocalModuleConfig.Mqtt.TlsEnabled)
        {
            builder = builder.WithTls()
                .WithTcpServer(host, Int32.Parse(port ?? "8883"));
        }
        else {
            builder = builder.WithTcpServer(host, Int32.Parse(port ?? "1883"));
        }

        if (container.LocalModuleConfig.Mqtt.Username is not null)
            builder = builder.WithCredentials(container.LocalModuleConfig.Mqtt.Username, container.LocalModuleConfig.Mqtt.Password);

        return builder.Build();
    }
}
