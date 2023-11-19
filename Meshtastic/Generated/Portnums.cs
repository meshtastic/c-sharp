// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: meshtastic/portnums.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Meshtastic.Protobufs {

  /// <summary>Holder for reflection information generated from meshtastic/portnums.proto</summary>
  public static partial class PortnumsReflection {

    #region Descriptor
    /// <summary>File descriptor for meshtastic/portnums.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static PortnumsReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChltZXNodGFzdGljL3BvcnRudW1zLnByb3RvEgptZXNodGFzdGljKtQDCgdQ",
            "b3J0TnVtEg8KC1VOS05PV05fQVBQEAASFAoQVEVYVF9NRVNTQUdFX0FQUBAB",
            "EhcKE1JFTU9URV9IQVJEV0FSRV9BUFAQAhIQCgxQT1NJVElPTl9BUFAQAxIQ",
            "CgxOT0RFSU5GT19BUFAQBBIPCgtST1VUSU5HX0FQUBAFEg0KCUFETUlOX0FQ",
            "UBAGEh8KG1RFWFRfTUVTU0FHRV9DT01QUkVTU0VEX0FQUBAHEhAKDFdBWVBP",
            "SU5UX0FQUBAIEg0KCUFVRElPX0FQUBAJEhgKFERFVEVDVElPTl9TRU5TT1Jf",
            "QVBQEAoSDQoJUkVQTFlfQVBQECASEQoNSVBfVFVOTkVMX0FQUBAhEg4KClNF",
            "UklBTF9BUFAQQBIVChFTVE9SRV9GT1JXQVJEX0FQUBBBEhIKDlJBTkdFX1RF",
            "U1RfQVBQEEISEQoNVEVMRU1FVFJZX0FQUBBDEgsKB1pQU19BUFAQRBIRCg1T",
            "SU1VTEFUT1JfQVBQEEUSEgoOVFJBQ0VST1VURV9BUFAQRhIUChBORUlHSEJP",
            "UklORk9fQVBQEEcSEAoLUFJJVkFURV9BUFAQgAISEwoOQVRBS19GT1JXQVJE",
            "RVIQgQISCAoDTUFYEP8DQl0KE2NvbS5nZWVrc3ZpbGxlLm1lc2hCCFBvcnRu",
            "dW1zWiJnaXRodWIuY29tL21lc2h0YXN0aWMvZ28vZ2VuZXJhdGVkqgIUTWVz",
            "aHRhc3RpYy5Qcm90b2J1ZnO6AgBiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Meshtastic.Protobufs.PortNum), }, null, null));
    }
    #endregion

  }
  #region Enums
  /// <summary>
  ///
  /// For any new 'apps' that run on the device or via sister apps on phones/PCs they should pick and use a
  /// unique 'portnum' for their application.
  /// If you are making a new app using meshtastic, please send in a pull request to add your 'portnum' to this
  /// master table.
  /// PortNums should be assigned in the following range:
  /// 0-63   Core Meshtastic use, do not use for third party apps
  /// 64-127 Registered 3rd party apps, send in a pull request that adds a new entry to portnums.proto to  register your application
  /// 256-511 Use one of these portnums for your private applications that you don't want to register publically
  /// All other values are reserved.
  /// Note: This was formerly a Type enum named 'typ' with the same id #
  /// We have change to this 'portnum' based scheme for specifying app handlers for particular payloads.
  /// This change is backwards compatible by treating the legacy OPAQUE/CLEAR_TEXT values identically.
  /// </summary>
  public enum PortNum {
    /// <summary>
    ///
    /// Deprecated: do not use in new code (formerly called OPAQUE)
    /// A message sent from a device outside of the mesh, in a form the mesh does not understand
    /// NOTE: This must be 0, because it is documented in IMeshService.aidl to be so
    /// ENCODING: binary undefined
    /// </summary>
    [pbr::OriginalName("UNKNOWN_APP")] UnknownApp = 0,
    /// <summary>
    ///
    /// A simple UTF-8 text message, which even the little micros in the mesh
    /// can understand and show on their screen eventually in some circumstances
    /// even signal might send messages in this form (see below)
    /// ENCODING: UTF-8 Plaintext (?)
    /// </summary>
    [pbr::OriginalName("TEXT_MESSAGE_APP")] TextMessageApp = 1,
    /// <summary>
    ///
    /// Reserved for built-in GPIO/example app.
    /// See remote_hardware.proto/HardwareMessage for details on the message sent/received to this port number
    /// ENCODING: Protobuf
    /// </summary>
    [pbr::OriginalName("REMOTE_HARDWARE_APP")] RemoteHardwareApp = 2,
    /// <summary>
    ///
    /// The built-in position messaging app.
    /// Payload is a [Position](/docs/developers/protobufs/api#position) message
    /// ENCODING: Protobuf
    /// </summary>
    [pbr::OriginalName("POSITION_APP")] PositionApp = 3,
    /// <summary>
    ///
    /// The built-in user info app.
    /// Payload is a [User](/docs/developers/protobufs/api#user) message
    /// ENCODING: Protobuf
    /// </summary>
    [pbr::OriginalName("NODEINFO_APP")] NodeinfoApp = 4,
    /// <summary>
    ///
    /// Protocol control packets for mesh protocol use.
    /// Payload is a [Routing](/docs/developers/protobufs/api#routing) message
    /// ENCODING: Protobuf
    /// </summary>
    [pbr::OriginalName("ROUTING_APP")] RoutingApp = 5,
    /// <summary>
    ///
    /// Admin control packets.
    /// Payload is a [AdminMessage](/docs/developers/protobufs/api#adminmessage) message
    /// ENCODING: Protobuf
    /// </summary>
    [pbr::OriginalName("ADMIN_APP")] AdminApp = 6,
    /// <summary>
    ///
    /// Compressed TEXT_MESSAGE payloads.
    /// ENCODING: UTF-8 Plaintext (?) with Unishox2 Compression
    /// NOTE: The Device Firmware converts a TEXT_MESSAGE_APP to TEXT_MESSAGE_COMPRESSED_APP if the compressed
    /// payload is shorter. There's no need for app developers to do this themselves. Also the firmware will decompress
    /// any incoming TEXT_MESSAGE_COMPRESSED_APP payload and convert to TEXT_MESSAGE_APP.
    /// </summary>
    [pbr::OriginalName("TEXT_MESSAGE_COMPRESSED_APP")] TextMessageCompressedApp = 7,
    /// <summary>
    ///
    /// Waypoint payloads.
    /// Payload is a [Waypoint](/docs/developers/protobufs/api#waypoint) message
    /// ENCODING: Protobuf
    /// </summary>
    [pbr::OriginalName("WAYPOINT_APP")] WaypointApp = 8,
    /// <summary>
    ///
    /// Audio Payloads.
    /// Encapsulated codec2 packets. On 2.4 GHZ Bandwidths only for now
    /// ENCODING: codec2 audio frames
    /// NOTE: audio frames contain a 3 byte header (0xc0 0xde 0xc2) and a one byte marker for the decompressed bitrate.
    /// This marker comes from the 'moduleConfig.audio.bitrate' enum minus one.
    /// </summary>
    [pbr::OriginalName("AUDIO_APP")] AudioApp = 9,
    /// <summary>
    ///
    /// Same as Text Message but originating from Detection Sensor Module.
    /// NOTE: This portnum traffic is not sent to the public MQTT starting at firmware version 2.2.9
    /// </summary>
    [pbr::OriginalName("DETECTION_SENSOR_APP")] DetectionSensorApp = 10,
    /// <summary>
    ///
    /// Provides a 'ping' service that replies to any packet it receives.
    /// Also serves as a small example module.
    /// ENCODING: ASCII Plaintext
    /// </summary>
    [pbr::OriginalName("REPLY_APP")] ReplyApp = 32,
    /// <summary>
    ///
    /// Used for the python IP tunnel feature
    /// ENCODING: IP Packet. Handled by the python API, firmware ignores this one and pases on.
    /// </summary>
    [pbr::OriginalName("IP_TUNNEL_APP")] IpTunnelApp = 33,
    /// <summary>
    ///
    /// Provides a hardware serial interface to send and receive from the Meshtastic network.
    /// Connect to the RX/TX pins of a device with 38400 8N1. Packets received from the Meshtastic
    /// network is forwarded to the RX pin while sending a packet to TX will go out to the Mesh network.
    /// Maximum packet size of 240 bytes.
    /// Module is disabled by default can be turned on by setting SERIAL_MODULE_ENABLED = 1 in SerialPlugh.cpp.
    /// ENCODING: binary undefined
    /// </summary>
    [pbr::OriginalName("SERIAL_APP")] SerialApp = 64,
    /// <summary>
    ///
    /// STORE_FORWARD_APP (Work in Progress)
    /// Maintained by Jm Casler (MC Hamster) : jm@casler.org
    /// ENCODING: Protobuf
    /// </summary>
    [pbr::OriginalName("STORE_FORWARD_APP")] StoreForwardApp = 65,
    /// <summary>
    ///
    /// Optional port for messages for the range test module.
    /// ENCODING: ASCII Plaintext
    /// NOTE: This portnum traffic is not sent to the public MQTT starting at firmware version 2.2.9
    /// </summary>
    [pbr::OriginalName("RANGE_TEST_APP")] RangeTestApp = 66,
    /// <summary>
    ///
    /// Provides a format to send and receive telemetry data from the Meshtastic network.
    /// Maintained by Charles Crossan (crossan007) : crossan007@gmail.com
    /// ENCODING: Protobuf
    /// </summary>
    [pbr::OriginalName("TELEMETRY_APP")] TelemetryApp = 67,
    /// <summary>
    ///
    /// Experimental tools for estimating node position without a GPS
    /// Maintained by Github user a-f-G-U-C (a Meshtastic contributor)
    /// Project files at https://github.com/a-f-G-U-C/Meshtastic-ZPS
    /// ENCODING: arrays of int64 fields
    /// </summary>
    [pbr::OriginalName("ZPS_APP")] ZpsApp = 68,
    /// <summary>
    ///
    /// Used to let multiple instances of Linux native applications communicate
    /// as if they did using their LoRa chip.
    /// Maintained by GitHub user GUVWAF.
    /// Project files at https://github.com/GUVWAF/Meshtasticator
    /// ENCODING: Protobuf (?)
    /// </summary>
    [pbr::OriginalName("SIMULATOR_APP")] SimulatorApp = 69,
    /// <summary>
    ///
    /// Provides a traceroute functionality to show the route a packet towards
    /// a certain destination would take on the mesh.
    /// ENCODING: Protobuf
    /// </summary>
    [pbr::OriginalName("TRACEROUTE_APP")] TracerouteApp = 70,
    /// <summary>
    ///
    /// Aggregates edge info for the network by sending out a list of each node's neighbors
    /// ENCODING: Protobuf
    /// </summary>
    [pbr::OriginalName("NEIGHBORINFO_APP")] NeighborinfoApp = 71,
    /// <summary>
    ///
    /// Private applications should use portnums >= 256.
    /// To simplify initial development and testing you can use "PRIVATE_APP"
    /// in your code without needing to rebuild protobuf files (via [regen-protos.sh](https://github.com/meshtastic/firmware/blob/master/bin/regen-protos.sh))
    /// </summary>
    [pbr::OriginalName("PRIVATE_APP")] PrivateApp = 256,
    /// <summary>
    ///
    /// ATAK Forwarder Module https://github.com/paulmandal/atak-forwarder
    /// ENCODING: libcotshrink
    /// </summary>
    [pbr::OriginalName("ATAK_FORWARDER")] AtakForwarder = 257,
    /// <summary>
    ///
    /// Currently we limit port nums to no higher than this value
    /// </summary>
    [pbr::OriginalName("MAX")] Max = 511,
  }

  #endregion

}

#endregion Designer generated code
