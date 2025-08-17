# Meshtastic Integration Tests

This project contains integration tests that require actual Meshtastic hardware devices connected via serial port. These tests are designed to verify real-world functionality of the Meshtastic C# library.

## Prerequisites

- A Meshtastic device connected via USB/serial port
- The device should be configured and operational
- .NET 9.0 SDK

## Environment Variables

You can configure the tests using the following environment variables:

- `MESHTASTIC_SERIAL_PORT`: Specify the serial port to use (e.g., `COM3` on Windows, `/dev/ttyUSB0` on Linux, `/dev/cu.usbserial-...` on macOS)
- `MESHTASTIC_DEST_NODE`: Specify a destination node number for targeted messages (optional, defaults to broadcast)

## Running the Tests

### Command Line

```bash
# Run all integration tests
dotnet test Meshtastic.IntegrationTest

# Run with specific serial port
MESHTASTIC_SERIAL_PORT=/dev/ttyUSB0 dotnet test Meshtastic.IntegrationTest

# Run with destination node
MESHTASTIC_SERIAL_PORT=COM3 MESHTASTIC_DEST_NODE=123456789 dotnet test Meshtastic.IntegrationTest

# Run only hardware tests
dotnet test Meshtastic.IntegrationTest --filter Category=RequiresHardware
```

### Visual Studio / VS Code

1. Set the environment variables in your test runner configuration
2. Run tests normally through the test explorer

## Test Categories

The tests are organized using NUnit categories:

- `IntegrationTest`: All integration tests
- `RequiresHardware`: Tests that require actual hardware
- `SerialDevice`: Tests that specifically require serial connection

## Expected Device Configuration

For the tests to work properly, your Meshtastic device should:

1. Be powered on and operational
2. Have a valid configuration (channels, etc.)
3. Be connected via USB/serial
4. Have the primary channel configured for text messaging

## Troubleshooting

### No Serial Ports Found

- Ensure your device is connected and recognized by the OS
- Check that the device is not being used by another application
- Verify the USB/serial drivers are installed

### Test Timeouts

- Check that the device is powered on and responsive
- Verify the device has a valid configuration
- Ensure the destination node (if specified) exists and is reachable

### Permission Issues (Linux/macOS)

You may need to add your user to the appropriate group:

```bash
# Linux
sudo usermod -a -G dialout $USER

# macOS - usually no additional permissions needed
```
