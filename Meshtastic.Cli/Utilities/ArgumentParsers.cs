using System.CommandLine.Parsing;
using System.Text.RegularExpressions;

namespace Meshtastic.Cli.Utilities;

public static class ArgumentParsers
{
    public static uint? NodeIdParser(ArgumentResult result)
    {
        if (result.Tokens.Count == 0)
        {
            return null;
        }
        else if (result.Tokens.Count > 1)
        {
            result.ErrorMessage = $"Argument --{result.Argument.Name} expects one argument but got {result.Tokens.Count}";
            return null;
        }

        var nodeStr = result.Tokens[0].Value.ToLowerInvariant();

        var hexMatch = Regex.Match(nodeStr, "^(!|0x)(?<num>([a-f|0-9][a-f|0-9]){1,4})$");
        if (hexMatch.Success)
        {
            try
            {
                return Convert.ToUInt32("0x" + hexMatch.Groups["num"].Value, 16);
            }
            catch (Exception e)
            {
                result.ErrorMessage = $"Argument --{result.Argument.Name} can not be parsed. " +
                                      $"{e.Message}";
                return null;
            }
        }

        var decMatch = Regex.Match(nodeStr, "^(?<num>[0-9]+)$");
        if (decMatch.Success)
        {
            try
            {
                return Convert.ToUInt32(decMatch.Groups["num"].Value, 10);
            }
            catch (Exception e)
            {
                result.ErrorMessage = $"Argument --{result.Argument.Name} can not be parsed. " +
                                      $"{e.Message}";
                return null;
            }
        }

        result.ErrorMessage = $"Argument --{result.Argument.Name} can not be parsed. " +
                              "One of formats expected: 0xDEADBEEF, !DEADBEEF, 3735928559";
        return null;
    }
}
