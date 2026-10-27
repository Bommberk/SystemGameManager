namespace SystemGameManager.VoiceChat.Service;

using SystemGameManager.VoiceChat.Entity;

/// <summary>
/// Abstraktion für eine Voicechat-Anwendung. Für weitere Apps (Discord etc.) einfach
/// eine weitere Implementierung hinzufügen und im VoiceChatController registrieren.
/// </summary>
public interface IVoiceChatProvider
{
    string Name { get; }

    /// <summary>Liefert den aktuellen Zustand oder null, wenn die App nicht erreichbar ist.</summary>
    Task<VoiceChatState?> GetStateAsync(CancellationToken cancellationToken = default);
}
