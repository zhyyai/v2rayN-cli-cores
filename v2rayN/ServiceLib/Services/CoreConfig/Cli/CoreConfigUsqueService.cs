using ServiceLib.Models.CoreConfigs;
using System.Text;
using System.Text.Json;

namespace ServiceLib.Services.CoreConfig.Cli;

/// <summary>
/// Converts a UsqueConfig JSON file into the CLI argument string
/// passed directly to the usque binary (Cloudflare WARP open client).
/// </summary>
public class CoreConfigUsqueService
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

            UsqueConfig cfg;
            if (File.Exists(srcPath))
            {
                var json = await File.ReadAllTextAsync(srcPath);
                cfg = JsonSerializer.Deserialize<UsqueConfig>(json, _jsonOpts)
                      ?? new UsqueConfig();

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
                cfg = new UsqueConfig
                {
                    Socks5Port = node.Port > 0 ? node.Port : 1080,
                    PrivateKey = node.Password.IsNotEmpty() ? node.Password : null,
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
            ret.Msg     = string.Format(ResUI.SuccessfulConfiguration, "usque");
        }
        catch (Exception ex)
        {
            Logging.SaveLog("CoreConfigUsqueService", ex);
            ret.Msg = ResUI.FailedGenDefaultConfiguration;
        }
        return ret;
    }

    /// <summary>
    /// Build CLI arguments for usque.
    /// See: https://github.com/Diniboy1123/usque
    /// </summary>
    public static string BuildArguments(UsqueConfig cfg)
    {
        var sb = new StringBuilder();

        // SOCKS5 listen address
        if (cfg.Socks5Port > 0)
            sb.Append($"--socks5-listen 127.0.0.1:{cfg.Socks5Port} ");

        if (cfg.PrivateKey.IsNotEmpty())
            sb.Append($"--private-key {cfg.PrivateKey} ");

        if (cfg.PublicKey.IsNotEmpty())
            sb.Append($"--public-key {cfg.PublicKey} ");

        if (cfg.Endpoint.IsNotEmpty())
            sb.Append($"--endpoint {cfg.Endpoint} ");

        if (cfg.License.IsNotEmpty())
            sb.Append($"--license {cfg.License} ");

        if (cfg.ExtraArgs.IsNotEmpty())
            sb.Append(cfg.ExtraArgs!.Trim());

        return sb.ToString().TrimEnd();
    }
}
