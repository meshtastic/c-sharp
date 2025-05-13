using Meshtastic.Protobufs;

namespace Meshtastic.Cli.Parsers;

public class UrlParser
{
    private readonly string url;

    public UrlParser(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException($"'{nameof(url)}' must be provided.", nameof(url));

        this.url = url;
    }

    public ChannelSet ParseChannels()
    {
        var split = this.url.Split("/#");
        var base64ChannelSet = split.Last();
        base64ChannelSet = base64ChannelSet.Replace('-', '+').Replace('_', '/');
        var missingPadding = base64ChannelSet.Length % 4;
        if (missingPadding > 0)
            base64ChannelSet += new string('=', 4 - missingPadding);

        var base64EncodedBytes = Convert.FromBase64String(base64ChannelSet);
        return ChannelSet.Parser.ParseFrom(base64EncodedBytes);
    }

    public SharedContact ParseContact()
    {
        var split = this.url.Split("/#");
        var content = split.Last();

        var base64EncodedBytes = Convert.FromBase64String(content);
        return SharedContact.Parser.ParseFrom(base64EncodedBytes);
    }
}
