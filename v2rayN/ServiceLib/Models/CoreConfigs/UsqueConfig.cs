namespace ServiceLib.Models.CoreConfigs;

/// <summary>
/// usque core configuration (Cloudflare WARP open-source client).
/// Serialize to JSON and store as ProfileItem.Address file (EConfigType.Custom + ECoreType.usque).
///
/// Example:
///   { "socks5Port": 1080, "privateKey": "...", "publicKey": "..." }
///
/// usque CLI reference: https://github.com/Diniboy1123/usque
/// </summary>
[Serializable]
public class UsqueConfig
{
    /// <summary>Local SOCKS5 listen port (--socks5-listen). Default 1080.</summary>
    public int Socks5Port { get; set; } = 1080;

    /// <summary>WireGuard private key for WARP registration (--private-key).</summary>
    public string? PrivateKey { get; set; }

    /// <summary>WARP peer public key (--public-key). Defaults to Cloudflare's well-known key.</summary>
    public string? PublicKey { get; set; }

    /// <summary>WARP endpoint address (--endpoint), e.g. "engage.cloudflareclient.com:2408".</summary>
    public string? Endpoint { get; set; }

    /// <summary>WARP license/token if using WARP+ (--license).</summary>
    public string? License { get; set; }

    /// <summary>Any additional raw CLI arguments appended at the end.</summary>
    public string? ExtraArgs { get; set; }
}
