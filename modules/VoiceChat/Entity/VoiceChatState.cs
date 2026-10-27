namespace SystemGameManager.VoiceChat.Entity;

/// <summary>
/// Zustand des Nutzers in einer Voicechat-Anwendung.
/// </summary>
public record VoiceChatState(
    string AppName,
    bool IsInVoiceChat,
    bool IsMicrophoneMuted,
    bool IsSoundMuted)
{
    /// <summary>Full mute: weder sprechen noch hören möglich.</summary>
    public bool IsFullMuted => IsInVoiceChat && IsMicrophoneMuted && IsSoundMuted;

    public static VoiceChatState NotInVoiceChat(string appName) => new(appName, false, false, false);
}
