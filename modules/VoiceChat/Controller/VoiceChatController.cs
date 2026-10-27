namespace SystemGameManager.VoiceChat.Controller;

using SystemGameManager.VoiceChat.Entity;
using SystemGameManager.VoiceChat.Service;

public class VoiceChatController
{
    private readonly List<IVoiceChatProvider> providers;

    public VoiceChatController()
        : this(new IVoiceChatProvider[] { new TeamSpeakVoiceChatProvider() })
    {
    }

    public VoiceChatController(IEnumerable<IVoiceChatProvider> providers)
    {
        this.providers = providers.ToList();
    }

    /// <summary>
    /// Liefert den Zustand der ersten Anwendung, in der der Nutzer in einem Voicechat ist, sonst null.
    /// </summary>
    public async Task<VoiceChatState?> GetActiveVoiceChatStateAsync(CancellationToken cancellationToken = default)
    {
        foreach (var provider in providers)
        {
            var state = await provider.GetStateAsync(cancellationToken);
            mlog($"VoiceChat: Provider '{provider.Name}' -> {(state is null ? "nicht erreichbar" : state.IsInVoiceChat ? "im Voicechat" : "nicht im Voicechat")}");
            if (state is { IsInVoiceChat: true })
                return state;
        }
        mlog("VoiceChat: In keiner unterstützten Anwendung im Voicechat.");
        return null;
    }

    public async Task<bool> IsInVoiceChatAsync(CancellationToken cancellationToken = default)
        => await GetActiveVoiceChatStateAsync(cancellationToken) != null;

    public async Task<bool> IsMicrophoneMutedAsync(CancellationToken cancellationToken = default)
        => (await GetActiveVoiceChatStateAsync(cancellationToken))?.IsMicrophoneMuted ?? false;

    public async Task<bool> IsFullMutedAsync(CancellationToken cancellationToken = default)
        => (await GetActiveVoiceChatStateAsync(cancellationToken))?.IsFullMuted ?? false;
}
