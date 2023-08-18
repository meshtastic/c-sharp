using Meshtastic.Protobufs;

namespace Meshtastic.Cli.Mappings;

internal static class HardwareModelMappings
{
    public static readonly IEnumerable<HardwareModel> NrfHardwareModels = new[]
    {
        HardwareModel.TEcho,
        HardwareModel.Rak4631,
        HardwareModel.NanoG2Ultra,
        HardwareModel.Nrf52Unknown
    };

    public static readonly Dictionary<HardwareModel, string> FileNameMappings = new()
    {
        { HardwareModel.HeltecV1, "heltec-v1" },
        { HardwareModel.HeltecV20, "heltec-v2.0" },
        { HardwareModel.HeltecV21, "heltec-v2.1" },
        { HardwareModel.HeltecV3, "heltec-v3" },
        { HardwareModel.HeltecWslV3, "heltec-wsl-v3" },
        { HardwareModel.M5Stack, "m5stack-core" },
        { HardwareModel.DiyV1, "meshtastic-diy-v1" },
        { HardwareModel.DrDev, "meshtastic-dr-dev" },
        { HardwareModel.NanoG1, "nano-g1" },
        { HardwareModel.Rak11200, "rak11200" },
        { HardwareModel.Rak4631, "rak4631" },
        { HardwareModel.StationG1, "station-g1" },
        { HardwareModel.Tbeam, "tbeam" },
        { HardwareModel.TbeamV0P7, "tbeam0.7" },
        { HardwareModel.LilygoTbeamS3Core, "tbeam-s3-core" },
        { HardwareModel.TloraV1, "tlora-v1" },
        { HardwareModel.TloraV11P3, "tlora-v1_3" },
        { HardwareModel.TloraV2, "tlora-v2" },
        { HardwareModel.TloraV211P6, "tlora-v2-1-1.6" },
        { HardwareModel.TloraV211P8, "tlora-v2-1-1.8" },
        { HardwareModel.TEcho, "t-echo" },
    };
}
