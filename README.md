# Meshtastic C#
C# / .NET 7 based command line interface for meshtastic

```
Description:
  Meshtastic.Cli

Usage:
  Meshtastic.Cli [command] [options]

Options:
  --port <port>                                                    Target serial port for meshtastic device
  --host <host>                                                    Target host ip or name for meshtastic device
  --output <Console|Json>                                          Type of output format for the command
  -l, --log <Critical|Debug|Error|Information|None|Trace|Warning>  Logging level for command events [default: Information]
  --dest <dest>                                                    Destination node address for command
  -sd, --select-dest                                               Interactively select a destination address from device's node list for command [default:
                                                                   False]
  --version                                                        Show version information
  -?, -h, --help                                                   Show help and usage information

Commands:
  list                                   List available serial ports
  monitor                                Serial monitor for the device
  info                                   Dump info about the device
  get                                    Display one or more settings from the device
  set                                    Save one or more settings onto the device
  channel <Add|Disable|Enable|Save>      Enable, Disable, Add, Save channels on the device
  url <Get|Set> <url>                    Get or set shared channel url []
  reboot <seconds>                       Reboot the device [default: 5]
  metadata                               Get device metadata from the device
  factory-reset                          Factory reset configuration of the device
  fixed-position <lat> <lon> <altitude>  Set the device to a fixed position [default: 0]
  text <message>                         Send a text message from the device
```

### Example of `Meshtastic.Cli info` output with default console output level
![image](https://user-images.githubusercontent.com/9000580/210158789-96f2c61f-1ed6-4ea0-97e0-187a27e89bd6.png)

## Installation (dotnet cli / tool method)

* Install the latest [dotnet 7 sdk](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) for your platform 
* Install the Meshtastic.Cli nuget package as a dotnet tool via `dotnet tool install --global Meshtastic.Cli` in your terminal of choice

## Installation (standalone executable)

* Navigate to the [Releases page](https://github.com/meshtastic/c-sharp/releases) in this github repsitory
* Download and extract the zip archive with the standalone executable for your platform

![image](https://user-images.githubusercontent.com/9000580/210138838-d3aced5e-1f5b-4881-9e4d-6677d7fc94ae.png)

* Execute it in the terminal of your choice (this may require security allowances or display warnings on some platforms)

## Updating via dotnet cli

* Execute `dotnet tool update Meshtastic.Cli -g` in your terminal of choice
