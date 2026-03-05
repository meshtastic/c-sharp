using Meshtastic.Discovery;

namespace Meshtastic.IntegrationTest.Tests.ConnectedDeviceTests;

[TestFixture]
[Category(TestCategories.RequiresHardware)]
[Category(TestCategories.SerialDevice)]
public class TextMessageTests : IntegrationTestBase
{
    private ILogger _logger = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Set up logging
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole()
                   .SetMinimumLevel(LogLevel.Debug));
        _logger = loggerFactory.CreateLogger<TextMessageTests>();
    }

    [Test]
    [CancelAfter(15000)] // 15 second timeout
    [TestCaseSource(nameof(GetDevicesUnderTest))]
    public async Task SendTextMessage_ShouldReceiveAck_WhenDeviceConnected(MeshtasticDevice device)
    {
        var testMessage = $"Integration test message at {DateTime.Now:HH:mm:ss}";
        var ackReceived = false;
        var errorReason = Routing.Types.Error.None;
        
        _logger.LogInformation($"Testing device: {device.Name} on {device.SerialPort}");
        
        var connection = new SerialConnection(_logger, device.SerialPort!);
        
        try
        {
            // First, get device configuration to establish connection
            var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
            var container = await connection.WriteToRadio(wantConfig, async (fromRadio, container) =>
            {
                // Complete when we have MyNodeInfo (minimum needed for sending messages)
                return await Task.FromResult(container.MyNodeInfo.MyNodeNum != 0);
            });
            
            container.ShouldNotBeNull();
            container.MyNodeInfo.ShouldNotBeNull();
            container.MyNodeInfo.MyNodeNum.ShouldNotBe(0u);
            
            _logger.LogInformation($"Connected to device. Node number: {container.MyNodeInfo.MyNodeNum}");
            
            // Create and send text message
            var textMessageFactory = new TextMessageFactory(container);
            var textMessage = textMessageFactory.CreateTextMessagePacket(testMessage);
            
            _logger.LogInformation($"Sending text message: '{testMessage}'");
            
            // Send message and wait for ACK
            var toRadioFactory = new ToRadioMessageFactory();
            await connection.WriteToRadio(toRadioFactory.CreateMeshPacketMessage(textMessage),
                async (fromRadio, container) =>
                {
                    var routingResult = fromRadio.GetPayload<Routing>();
                    if (routingResult != null && fromRadio.Packet?.Priority == MeshPacket.Types.Priority.Ack)
                    {
                        errorReason = routingResult.ErrorReason;
                        ackReceived = true;
                        
                        if (routingResult.ErrorReason == Routing.Types.Error.None)
                            _logger.LogInformation("✅ Message acknowledged successfully");
                        else
                            _logger.LogWarning($"❌ Message delivery failed: {routingResult.ErrorReason}");
                        
                        return await Task.FromResult(true);
                    }
                    
                    // Log other incoming messages for debugging
                    if (fromRadio.Packet != null)
                    {
                        _logger.LogDebug($"Received packet: {fromRadio.Packet.Decoded?.Portnum} Priority: {fromRadio.Packet.Priority}");
                    }
                    
                    return await Task.FromResult(false);
                });
        }
        finally
        {
            connection.Disconnect();
        }
        
        // Assert
        ackReceived.ShouldBeTrue("An ACK should be received for the sent message");
        errorReason.ShouldBe(Routing.Types.Error.None, "The message should be delivered without errors");
    }
    
    [Test]
    [CancelAfter(15000)] // 15 second timeout
    [TestCaseSource(nameof(GetDevicesUnderTest))]
    public async Task GetDeviceInfo_ShouldReturnValidNodeInfo(MeshtasticDevice device)
    {
        // Arrange & Act
        _logger.LogInformation($"Testing device info for: {device.Name} on {device.SerialPort}");
        
        var connection = new SerialConnection(_logger, device.SerialPort!);
        
        DeviceStateContainer? container = null;
        
        try
        {
            var toRadioFactory = new ToRadioMessageFactory();
            var wantConfig = toRadioFactory.CreateWantConfigMessage();
            container = await connection.WriteToRadio(wantConfig, async (fromRadio, container) =>
            {
                // Complete when we have sufficient device info
                var deviceNode = container.GetDeviceNodeInfo();
                return await Task.FromResult(
                    container.MyNodeInfo.MyNodeNum != 0 && 
                    deviceNode != null &&
                    !string.IsNullOrEmpty(deviceNode.User?.LongName));
            });
        }
        finally
        {
            connection.Disconnect();
        }
        
        // Assert
        container.ShouldNotBeNull();
        container!.MyNodeInfo.ShouldNotBeNull();
        container.MyNodeInfo.MyNodeNum.ShouldNotBe(0u);
        
        var deviceNode = container.GetDeviceNodeInfo();
        deviceNode.ShouldNotBeNull();
        deviceNode!.User.ShouldNotBeNull();
        deviceNode.User!.LongName.ShouldNotBeNullOrEmpty();
        
        _logger.LogInformation($"Device Info - Node: {container.MyNodeInfo.MyNodeNum}, " +
                             $"Name: '{deviceNode.User.LongName}', " +
                             $"Short: '{deviceNode.User.ShortName}'");
    }
}
