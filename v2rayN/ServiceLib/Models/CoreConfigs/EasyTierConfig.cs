namespace ServiceLib.Models.CoreConfigs;

/// <summary>
/// EasyTier core configuration.
/// Serialize this to JSON and store as the ProfileItem.Address file (EConfigType.Custom + ECoreType.easytier).
/// 
/// Example — local SOCKS5 mode (PC side):
///   { "virtualIp": "10.144.144.1", "peers": ["wss://ws.example.com:0"], "socks5Port": 20808, "noTun": true, "network": "0.0.0.0/0" }
///
/// Example — remote SOCKS5 mode (PC side, port-forward to cloud SOCKS5):
///   { "virtualIp": "10.144.144.1", "peers": ["wss://ws.example.com:0"], "portForwards": ["tcp://127.0.0.1:20808/10.144.144.2:8080"], "noTun": true }
///
/// Example — cloud side (remote SOCKS5 server):
///   { "virtualIp": "10.144.144.2", "noListener": true, "peers": ["wss://ws.example.com:0"], "socks5Port": 8080, "noTun": true, "network": "0.0.0.0/0" }
/// </summary>
[Serializable]
public class EasyTierConfig
{
    /// <summary>Virtual IP for this node, e.g. "10.144.144.1"</summary>
    public string? VirtualIp { get; set; }

    /// <summary>Peer/relay URLs, e.g. ["wss://ws.example.com:0", "tcp://1.2.3.4:11010"]</summary>
    public List<string> Peers { get; set; } = [];

    /// <summary>Local SOCKS5 proxy port. 0 = disabled. e.g. 20808</summary>
    public int Socks5Port { get; set; } = 0;

    /// <summary>Port-forward rules, e.g. ["tcp://127.0.0.1:20808/10.144.144.2:8080"]</summary>
    public List<string> PortForwards { get; set; } = [];

    /// <summary>Disable TUN device (--no-tun). Recommended when only SOCKS5 is needed.</summary>
    public bool NoTun { get; set; } = true;

    /// <summary>Disable listener (--no-listener). Used on cloud side behind relay.</summary>
    public bool NoListener { get; set; } = false;

    /// <summary>Subnet routing (-n), e.g. "0.0.0.0/0" to route all traffic via this node.</summary>
    public string? Network { get; set; }

    /// <summary>Any additional raw CLI arguments appended at the end.</summary>
    public string? ExtraArgs { get; set; }
}
