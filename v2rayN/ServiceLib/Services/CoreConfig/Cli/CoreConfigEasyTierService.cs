using ServiceLib.Models.CoreConfigs;
using System.Text;
using System.Text.Json;

namespace ServiceLib.Services.CoreConfig.Cli;

/// <summary>
/// Converts an EasyTierConfig JSON file into the CLI argument string
/// passed directly to the easytier-core binary.
/// </summary>
public class CoreConfigEasyTierService
{
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Generate (or copy) the working config JSON at <paramref name="destFileName"/>
    /// and return a RetResult whose Data contains the CLI argument string.
    /// </summary>
    public async Task<RetResult> GenerateClientConfig(ProfileItem node, string? destFileName)
    {
        var ret = new RetResult();
        try
        {
            // Resolve source config file from node.Address
            var srcPath = node.Address;
            if (!File.Exists(srcPath))
                srcPath = Utils.GetConfigPath(srcPath);

            EasyTierConfig cfg;
            if (File.Exists(srcPath))
            {
                var json = await File.ReadAllTextAsync(srcPath);
                cfg = JsonSerializer.Deserialize<EasyTierConfig>(json, _jsonOpts)
                      ?? new EasyTierConfig();

                // Copy to working dir so other components can inspect it
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
                // No file — build a default config and write it
                cfg = new EasyTierConfig
                {
                    VirtualIp    = node.Sni.IsNotEmpty() ? node.Sni : "10.144.144.1",
                    Peers        = node.Password.IsNotEmpty() ? [node.Password!] : [],
                    Socks5Port   = node.Port > 0 ? node.Port : 20808,
                    NoTun        = true,
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
            ret.Msg     = string.Format(ResUI.SuccessfulConfiguration, "EasyTier");
        }
        catch (Exception ex)
        {
            Logging.SaveLog("CoreConfigEasyTierService", ex);
            ret.Msg = ResUI.FailedGenDefaultConfiguration;
        }
        return ret;
    }

    /// <summary>Build CLI argument string from <paramref name="cfg"/>.</summary>
    public static string BuildArguments(EasyTierConfig cfg)
    {
        var sb = new StringBuilder();

        if (cfg.NoListener)
            sb.Append("--no-listener ");

        if (cfg.VirtualIp.IsNotEmpty())
            sb.Append($"-i {cfg.VirtualIp} ");

        foreach (var peer in cfg.Peers)
            sb.Append($"-p {peer} ");

        if (cfg.Socks5Port > 0)
            sb.Append($"--socks5 {cfg.Socks5Port} ");

        foreach (var pf in cfg.PortForwards)
            sb.Append($"--port-forward {pf} ");

        if (cfg.NoTun)
            sb.Append("--no-tun ");

        if (cfg.Network.IsNotEmpty())
            sb.Append($"-n {cfg.Network} ");

        if (cfg.ExtraArgs.IsNotEmpty())
            sb.Append(cfg.ExtraArgs!.Trim());

        return sb.ToString().TrimEnd();
    }
}
