# Meshtastic C#

![GitHub release downloads](https://img.shields.io/github/downloads/meshtastic/c-sharp/total)
[![Coverage Status](https://coveralls.io/repos/github/meshtastic/c-sharp/badge.svg)](https://coveralls.io/github/meshtastic/c-sharp)
[![CI](https://img.shields.io/github/actions/workflow/status/meshtastic/c-sharp/ci.yml?branch=master&label=actions&logo=github&color=yellow)](https://github.com/meshtastic/c-sharp/actions/workflows/ci.yml)
[![CLA assistant](https://cla-assistant.io/readme/badge/meshtastic/c-sharp)](https://cla-assistant.io/meshtastic/c-sharp)
[![Fiscal Contributors](https://opencollective.com/meshtastic/tiers/badge.svg?label=Fiscal%20Contributors&color=deeppink)](https://opencollective.com/meshtastic/)
[![Vercel](https://img.shields.io/static/v1?label=Powered%20by&message=Vercel&style=flat&logo=vercel&color=000000)](https://vercel.com?utm_source=meshtastic&utm_campaign=oss)

## Overview
C# / .NET 7 based command line interface for meshtastic



## Stats

![Alt](https://repobeats.axiom.co/api/embed/d563d12d9eb01ed9f875ad9c47dac64cd5fc521c.svg "Repobeats analytics image")

## Installation & Usage

```
Description:
  Meshtastic.Cli

Usage:
  Meshtastic.Cli [command] [options]

Options:
  --port <port>                                          Target serial port for meshtastic device
  --host <host>                                          Target host ip or name for meshtastic device
  --output <Json|PrettyConsole>                          Type of output format for the command
  -l, --log                                              Logging level for command events [default: Information]
  <Critical|Debug|Error|Information|None|Trace|Warning>
  --dest <dest>                                          Destination node address for command
  -sd, --select-dest                                     Interactively select a destination from device's node list
                                                         [default: False]
  --version                                              Show version information
  -?, -h, --help                                         Show help and usage information

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
  reset-nodedb                           Reset the node db of the device
  trace-route                            Trace the sequence of nodes routing to the destination
  canned-messages <Get|Set> <messages>   Get or set the collection of canned messages on the device [operation: Get, ]
  waypoint <lat> <lon>                   Send a waypoint from the device
  file <path>                            Get or send a file from the device
  update                                 Update the firmware of the serial connected device
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
