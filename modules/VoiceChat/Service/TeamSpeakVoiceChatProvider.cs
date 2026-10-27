namespace SystemGameManager.VoiceChat.Service;

using System.Net.Sockets;
using System.Text;
using SystemGameManager.VoiceChat.Entity;

/// <summary>
/// Liest den Voicechat-Zustand über die TeamSpeak-3-ClientQuery-API (Telnet, Standard 127.0.0.1:25639).
/// Benötigt den ClientQuery-API-Key (GlobalConfig.Settings.TeamSpeakConfig.ApiKey).
/// </summary>
public class TeamSpeakVoiceChatProvider : IVoiceChatProvider
{
    private const int TIMEOUT_MS = 2000;

    private readonly string host;
    private readonly int port;
    private readonly string apiKey;

    public string Name => "TeamSpeak";

    public TeamSpeakVoiceChatProvider(string? host = null, int? port = null, string? apiKey = null)
    {
        var config = GlobalConfig.Settings.TeamSpeakConfig;
        this.host = host ?? config.Host;
        this.port = port ?? config.Port;
        this.apiKey = apiKey ?? config.ApiKey;
    }

    public async Task<VoiceChatState?> GetStateAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return null;

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TIMEOUT_MS);

            using var client = new TcpClient();
            await client.ConnectAsync(host, port, timeout.Token);
            using var reader = new StreamReader(client.GetStream(), Encoding.UTF8);
            using var writer = new StreamWriter(client.GetStream(), new UTF8Encoding(false)) { NewLine = "\n", AutoFlush = true };

            // Begrüßungsbanner überspringen (endet mit der Zeile "selected schandlerid=...")
            await ReadUntilAsync(reader, l => l.StartsWith("selected schandlerid"), timeout.Token);

            var auth = await SendAsync(reader, writer, $"auth apikey={apiKey}", timeout.Token);
            if (!IsOk(auth))
                return null;

            var whoami = await SendAsync(reader, writer, "whoami", timeout.Token);
            if (!IsOk(whoami))
                return VoiceChatState.NotInVoiceChat(Name);

            var whoamiFields = ParseFields(whoami.FirstOrDefault(l => l.Contains("clid=")) ?? "");
            if (!whoamiFields.TryGetValue("clid", out var clid)
                || !whoamiFields.TryGetValue("cid", out var cid)
                || cid == "0" || string.IsNullOrEmpty(cid))
                return VoiceChatState.NotInVoiceChat(Name);

            var vars = await SendAsync(reader, writer,
                $"clientvariable clid={clid} client_input_muted client_output_muted", timeout.Token);
            if (!IsOk(vars))
                return null;

            var fields = ParseFields(vars.FirstOrDefault(l => l.Contains("client_input_muted")) ?? "");
            return new VoiceChatState(
                Name,
                true,
                fields.GetValueOrDefault("client_input_muted") == "1",
                fields.GetValueOrDefault("client_output_muted") == "1");
        }
        catch (Exception ex) when (ex is SocketException or IOException or OperationCanceledException)
        {
            return null;
        }
    }

    private static async Task<List<string>> SendAsync(StreamReader reader, StreamWriter writer, string command, CancellationToken token)
    {
        await writer.WriteLineAsync(command.AsMemory(), token);
        return await ReadUntilAsync(reader, l => l.StartsWith("error "), token);
    }

    private static async Task<List<string>> ReadUntilAsync(StreamReader reader, Func<string, bool> isEnd, CancellationToken token)
    {
        var lines = new List<string>();
        while (true)
        {
            var line = (await reader.ReadLineAsync(token))?.Trim();
            if (line == null)
                throw new IOException("Connection closed.");
            if (line.Length == 0)
                continue;
            lines.Add(line);
            if (isEnd(line))
                return lines;
        }
    }

    private static bool IsOk(List<string> response) =>
        response.LastOrDefault()?.Contains("id=0") == true;

    private static Dictionary<string, string> ParseFields(string line)
    {
        var result = new Dictionary<string, string>();
        foreach (var part in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = part.IndexOf('=');
            if (idx > 0)
                result[part[..idx]] = part[(idx + 1)..];
        }
        return result;
    }
}
