namespace SystemGameManager.Handler;

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Web.WebView2.WinForms;
using SystemGameManager.Games.Entity;
using SystemGameManager.AudioManager.Service;
using SystemGameManager.Plugin;
using SystemGameManager.Plugin.HotKeys;
using SystemGameManager.User.Entity;
using SystemGameManager.User.Service;
public static class WebApiHandler
{
    public static void Initialize(WebView2 web, Action<User>? onUserSettingsUpdated = null)
    {
        web.CoreWebView2.WebMessageReceived += async (_, e) =>
        {
            var request = JsonSerializer.Deserialize<ApiRequest>(e.WebMessageAsJson);

            if (request == null)
                return;

            if (request.Action.StartsWith("get"))
            {
                await getMethod(request, web);
            }
            else if (request.Action.StartsWith("set"))
            {
                await setMethod(request, onUserSettingsUpdated);
            }else
            {
                if(request.Action == "changeGameImage")
                {
                    await ChangeGameImage(request, web);
                }else if(request.Action == "removeGameFromView")
                {
                    await RemoveGameFromView(request);
                }
                else if(request.Action == "startHotKeyCapture")
                {
                    StartHotKeyCapture(web);
                }
                else if(request.Action == "saveHotKey")
                {
                    await SaveHotKey(request, web);
                }
                else if(request.Action == "deleteHotKey")
                {
                    await DeleteHotKey(request, web);
                }
                else if(request.Action == "selectDefaultWallpaper")
                {
                    await SelectDefaultWallpaper(web);
                }
                else if(request.Action == "selectLivelyExe")
                {
                    await SelectLivelyExe(web);
                }
                else
                {
                    MessageBox.Show($"Unknown action: {request.Action}");
                }
            }
        };
    }

    private static async Task setMethod(ApiRequest request, Action<User>? onUserSettingsUpdated)
    {
        switch (request.Action)
        {
            case "setGames":
                await SetGames(request);
                break;
            case "setLaunchers":
                await SetLaunchers(request);
                break;
            case "setScreenForGame":
                await SetScreenForGame(request);
                break;
            case "setUserName":
                await SetUserName(request);
                break;
            case "setUserTheme":
                await SetUserTheme(request);
                break;
            case "setUserBackgroundSettings":
                SetUserBackgroundSettings(request, onUserSettingsUpdated);
                break;
            case "setUserWallpaperSettings":
                SetUserWallpaperSettings(request);
                break;
            default:
                MessageBox.Show($"Unknown set action: {request.Action}");
                break;
        }
    }
    private static async Task getMethod(ApiRequest request, WebView2 web)
    {
        switch (request.Action)
        {
            case "getGames":
                await GetGames(web);
                break;
            case "getLaunchers":
                await GetLaunchers(web);
                break;
            case "getAudioDevices":
                await GetAudioDevices(web);
                break;
            case "getScreens":
                await GetScreens(web);
                break;
            case "getScreenForGame":
                await GetScreenForGame(request, web);
                break;
            case "getHotKeys":
                await GetHotKeys(web);
                break;
            case "getUserSettings":
                await GetUserSettings(web);
                break;
            case "getDetectedWallpaperInfo":
                await GetDetectedWallpaperInfo(web);
                break;
            default:
                MessageBox.Show($"Unknown get action: {request.Action}");
                break;
        }
    }

    private static async Task GetGames(WebView2 web)
    {
        await Send(web, "getGames", Game.GetGames());
    }
    private static async Task GetLaunchers(WebView2 web)
    {
        await Send(web, "getLaunchers", Launcher.GetLaunchers());
    }
    private static async Task GetAudioDevices(WebView2 web)
    {
        await Send(web, "getAudioDevices", SystemAudioService.GetAudioOutputDeviceNames());
    }
    private static async Task GetScreens(WebView2 web)
    {
        await Send(web, "getScreens", Screen.AllScreens);
    }
    private static async Task GetScreenForGame(ApiRequest request, WebView2 web)
    {
        Game game = JsonSerializer.Deserialize<Game>(request.Data?.ToString() ?? "{}");
        await Send(web, "getScreenForGame", new { SerializedGameName = game.SerializedGameName, ScreenNumber = game.LivelyWallpaperScreen });   
    }
    
    private static async Task SetGames(ApiRequest request)
    {
        Game[] games = JsonSerializer.Deserialize<List<Game>>(request.Data?.ToString() ?? "[]").ToArray();
        Game.UpdateMultibleGames(games);
        // MessageBox.Show("Games were updated", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static async Task SetLaunchers(ApiRequest request)
    {
        
    }
    private static async Task SetScreenForGame(ApiRequest request)
    {
        Game game = JsonSerializer.Deserialize<Game>(request.Data?.ToString() ?? "{}");
        Game.UpdateGame(game);
        _ = Task.Run(() => LivelyWallpaper.Instance.ApplyNow(game));
        msgbox("Screen for game set successfully.");
    }

    private static async Task ChangeGameImage(ApiRequest request, WebView2 web)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Bilder|*.png;*.jpg;*.jpeg;*.webp;*.gif|Alle Dateien|*.*"
        };

        try{
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string path = dialog.FileName;
                if(request == null || request.Data == null)
                {
                    ErrorHandler.Handle(new Exception("No game data provided for changing image."), ErrorSeverity.Error);
                    return;
                }
                Game game = JsonSerializer.Deserialize<Game>(request.Data?.ToString() ?? "{}");
                game.GameImage = path;
                Game.UpdateGame(game);
                _ = Task.Run(() => LivelyWallpaper.Instance.ApplyNow(game));
                await Send(web, "gameImageChanged", new { SerializedGameName = game.SerializedGameName, GameImage = game.GameImage });
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, ErrorSeverity.Error);
        }
    }
    private static async Task RemoveGameFromView(ApiRequest request)
    {
        Game game = JsonSerializer.Deserialize<Game>(request.Data?.ToString() ?? "{}");
        game.IsRemovedFromView = true;
        Game.UpdateGame(game);
    }

    private static async Task GetHotKeys(WebView2 web)
    {
        var hotKeys = HotKey.GetHotKeys().Select(hotKey => new
        {
            hotKey.Name,
            hotKey.Description,
            hotKey.MusicVolumePercent,
            hotKey.GameVolumePercent,
            hotKey.AudioOutputDevice,
            hotKey.KeyCodes,
            hotKey.GameSerializedName,
            KeyDisplay = HotKeyListener.GetKeyDisplayName(hotKey.KeyCodes)
        });
        await Send(web, "getHotKeys", hotKeys);
    }

    private static async Task GetUserSettings(WebView2 web)
    {
        await Send(web, "getUserSettings", User.GetUser());
    }

    private static async Task SetUserName(ApiRequest request)
    {
        var settings = JsonSerializer.Deserialize<UserSettingsRequest>(request.Data?.ToString() ?? "{}");
        User.UpdateName(settings?.Name);
    }

    private static async Task SetUserTheme(ApiRequest request)
    {
        var settings = JsonSerializer.Deserialize<UserSettingsRequest>(request.Data?.ToString() ?? "{}");
        User.UpdateTheme(settings?.Theme);
    }

    private static void SetUserBackgroundSettings(ApiRequest request, Action<User>? onUserSettingsUpdated)
    {
        var settings = JsonSerializer.Deserialize<UserBackgroundSettingsRequest>(request.Data?.ToString() ?? "{}");
        var user = User.UpdateBackgroundSettings(
            settings?.EnableBackgroundRunning ?? false,
            settings?.CloseToBackground ?? false,
            settings?.StartWithWindows ?? false,
            settings?.StartInBackground ?? false);

        try
        {
            UserStartupService.Update(user);
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, ErrorSeverity.Warning);
        }

        onUserSettingsUpdated?.Invoke(user);
    }

    private static void SetUserWallpaperSettings(ApiRequest request)
    {
        var settings = JsonSerializer.Deserialize<UserWallpaperSettingsRequest>(request.Data?.ToString() ?? "{}");
        User.UpdateWallpaperSettings(settings?.DefaultWallpaperPath, settings?.LivelyExePath);
    }

    private static async Task SelectDefaultWallpaper(WebView2 web)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Bilder|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp|Alle Dateien|*.*"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        var user = User.GetUser();
        User.UpdateWallpaperSettings(dialog.FileName, user.LivelyExePath);
        await GetUserSettings(web);
        await GetDetectedWallpaperInfo(web);
    }

    private static async Task SelectLivelyExe(WebView2 web)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Lively CLI|livelycu.exe|Programme|*.exe"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        var user = User.GetUser();
        User.UpdateWallpaperSettings(user.DefaultWallpaperPath, dialog.FileName);
        await GetUserSettings(web);
        await GetDetectedWallpaperInfo(web);
    }

    private static async Task GetDetectedWallpaperInfo(WebView2 web)
    {
        var user = User.GetUser();
        await Send(web, "getDetectedWallpaperInfo", new
        {
            WindowsWallpaperPath = WindowsWallpaperService.StartupWallpaperPath,
            LivelyExePath = LivelyLocator.FindLivelyExe(user.LivelyExePath)
        });
    }

    private static Action<string>? hotKeyCaptureHandler;

    private static void StartHotKeyCapture(WebView2 web)
    {
        if (hotKeyCaptureHandler is not null)
        {
            HotKeyListener.KeysCaptured -= hotKeyCaptureHandler;
        }

        hotKeyCaptureHandler = keyCodes =>
        {
            // the hook runs on the UI thread, so the web view can be used directly
            _ = Send(web, "hotKeyCaptured", new { KeyCodes = keyCodes, KeyDisplay = HotKeyListener.GetKeyDisplayName(keyCodes) });
        };
        HotKeyListener.KeysCaptured += hotKeyCaptureHandler;
        HotKeyListener.StartCapture();
    }

    private static async Task SaveHotKey(ApiRequest request, WebView2 web)
    {
        var hotKey = JsonSerializer.Deserialize<HotKey>(request.Data?.ToString() ?? "{}");
        if (hotKey is null || string.IsNullOrWhiteSpace(hotKey.Name) || string.IsNullOrWhiteSpace(hotKey.KeyCodes))
            return;
        HotKey.Save(hotKey);
        await GetHotKeys(web);
    }

    private static async Task DeleteHotKey(ApiRequest request, WebView2 web)
    {
        var hotKey = JsonSerializer.Deserialize<HotKey>(request.Data?.ToString() ?? "{}");
        if (hotKey is null || string.IsNullOrWhiteSpace(hotKey.Name))
            return;
        HotKey.Delete(hotKey.Name);
        await GetHotKeys(web);
    }

    private static async Task Send(WebView2 web, string action, object data)
    {
        var json = JsonSerializer.Serialize(new
        {
            action,
            data,
        });

        await web.ExecuteScriptAsync($"""
            window.apiResponse({json});
        """);
    }

    private class ApiRequest
    {
        [JsonPropertyName("action")]
        public string? Action { get; set; }
        [JsonPropertyName("data")]
        public object? Data { get; set; }
    }

    private sealed class UserSettingsRequest
    {
        public string? Name { get; set; }
        public string? Theme { get; set; }
    }

    private sealed class UserWallpaperSettingsRequest
    {
        public string? DefaultWallpaperPath { get; set; }
        public string? LivelyExePath { get; set; }
    }

    private sealed class UserBackgroundSettingsRequest
    {
        public bool EnableBackgroundRunning { get; set; }
        public bool CloseToBackground { get; set; }
        public bool StartWithWindows { get; set; }
        public bool StartInBackground { get; set; }
    }
}