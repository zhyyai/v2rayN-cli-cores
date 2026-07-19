using ServiceLib.Models.CoreConfigs;
using System.Text;
using System.Text.Json;

namespace ServiceLib.Services.CoreConfig.Cli;

/// <summary>
/// Converts a NetBirdConfig JSON file into the CLI argument string
/// passed directly to the netbird binary.
///
/// Note: NetBird does not expose a native SOCKS5 port. The Socks5Port field
/// in the config is informational only — it tells v2rayN which port your
/// companion SOCKS5 daemon (e.g. microsocks / gost) is listening on the
/// NetBird virtual IP. Configure that daemon separately on the remote peer.
/// </summary>
public class CoreConfigNetBirdService
{
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public async Task<RetResult> GenerateClientConfig(ProfileItem node, string? destFileName)
    {
        var ret = new RetResult();
        try
        {
            var srcPath = node.Address;
            if (!File.Exists(srcPath))
                srcPath = Utils.GetConfigPath(srcPath);

            NetBirdConfig cfg;
            if (File.Exists(srcPath))
            {
                var json = await File.ReadAllTextAsync(srcPath);
                cfg = JsonSerializer.Deserialize<NetBirdConfig>(json, _jsonOpts)
                      ?? new NetBirdConfig();

                if (destFileName.IsNotEmpty() && srcPath != destFileName)
                {
                    if (File.Exists(destFileName))
                        File.SetAttributes(destFileName, FileAttributes.Normal);
                    File.Copy(srcPath, destFileName!, overwrite: true);
                    File.SetAttributes(destFileName!, FileAttributes.Normal);
                }
            }
            else
            {
                cfg = new NetBirdConfig
                {
                    SetupKey      = node.Password.IsNotEmpty() ? node.Password : null,
                    ManagementUrl = node.Address.IsNotEmpty() ? node.Address : "https://api.netbird.io:443",
                    Socks5Port    = node.Port > 0 ? node.Port : 1080,
                };
                if (destFileName.IsNotEmpty())
                {
                    var generated = JsonSerializer.Serialize(cfg,
                        new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(destFileName!, generated);
                }
            }

            ret.Data    = BuildArguments(cfg);
            ret.Success = true;
            ret.Msg     = string.Format(ResUI.SuccessfulConfiguration, "NetBird");
        }
        catch (Exception ex)
        {
            Logging.SaveLog("CoreConfigNetBirdService", ex);
            ret.Msg = ResUI.FailedGenDefaultConfiguration;
        }
        return ret;
    }

    /// <summary>
    /// Build CLI arguments for: netbird up ...
    /// See: https://github.com/netbirdio/netbird
    /// </summary>
    public static string BuildArguments(NetBirdConfig cfg)
    {
        var sb = new StringBuilder();

        // NetBird uses subcommand "up" to connect
        sb.Append("up ");

        if (cfg.ManagementUrl.IsNotEmpty())
            sb.Append($"--management-url {cfg.ManagementUrl} ");

        if (cfg.SetupKey.IsNotEmpty())
            sb.Append($"--setup-key {cfg.SetupKey} ");

        if (cfg.Hostname.IsNotEmpty())
            sb.Append($"--hostname {cfg.Hostname} ");

        if (cfg.PreSharedKey.IsNotEmpty())
            sb.Append($"--preshared-key {cfg.PreSharedKey} ");

        if (cfg.ExtraArgs.IsNotEmpty())
            sb.Append(cfg.ExtraArgs!.Trim());

        return sb.ToString().TrimEnd();
    }
}
