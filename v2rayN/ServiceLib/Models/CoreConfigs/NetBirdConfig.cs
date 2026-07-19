namespace ServiceLib.Models.CoreConfigs;

/// <summary>
/// NetBird core configuration (WireGuard-based mesh VPN).
/// Serialize to JSON and store as ProfileItem.Address file (EConfigType.Custom + ECoreType.netbird).
///
/// NetBird connects to a management server and creates a WireGuard-based overlay.
/// To expose a SOCKS5 proxy for v2rayN use, pair with --socks5-port (if supported by your build)
/// or route traffic through a peer that runs a SOCKS5 server on the NetBird virtual IP.
///
/// Example:
///   { "setupKey": "AAAA-BBBB-CCCC", "managementUrl": "https://api.netbird.io:443",
///     "socks5Port": 1080, "hostname": "my-pc" }
///
/// NetBird CLI reference: https://github.com/netbirdio/netbird
/// </summary>
[Serializable]
public class NetBirdConfig
{
    /// <summary>Management server URL (--management-url). Default: Netbird SaaS.</summary>
    public string ManagementUrl { get; set; } = "https://api.netbird.io:443";

    /// <summary>One-time enrollment setup key (--setup-key).</summary>
    public string? SetupKey { get; set; }

    /// <summary>
    /// Local SOCKS5 port exposed to v2rayN. NetBird itself does not expose SOCKS5.
    /// Set this when you run a separate SOCKS5 server (e.g. gost / microsocks) bound to
    /// the NetBird virtual IP, and want v2rayN to know which port to connect to.
    /// </summary>
    public int Socks5Port { get; set; } = 1080;

    /// <summary>Custom hostname for this peer (--hostname).</summary>
    public string? Hostname { get; set; }

    /// <summary>Pre-shared admin token for headless auth (--admin-url companion, optional).</summary>
    public string? PreSharedKey { get; set; }

    /// <summary>Any additional raw CLI arguments appended at the end.</summary>
    public string? ExtraArgs { get; set; }
}
