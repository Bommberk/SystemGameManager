namespace SystemGameManager.Handler;

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Web.WebView2.WinForms;
using SystemGameManager.Games.Entity;

public static class WebApiHandler
{
    public static void Initialize(WebView2 web)
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
                await setMethod(request);
            }else
            {
                MessageBox.Show($"Unknown action: {request.Action}");
            }
        };
    }

    private static async Task setMethod(ApiRequest request)
    {
        switch (request.Action)
        {
            case "setGames":
                await SetGames(request);
                break;
            case "setLaunchers":
                await SetLaunchers(request);
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
    
    private static async Task SetGames(ApiRequest request)
    {
        Game[] games = JsonSerializer.Deserialize<List<Game>>(request.Data?.ToString() ?? "[]").ToArray();
        Game.UpdateMultibleGames(games);
        MessageBox.Show("Games were updated", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static async Task SetLaunchers(ApiRequest request)
    {
        
    }

    private static async Task Send(WebView2 web, string action, object data)
    {
        var json = JsonSerializer.Serialize(new
        {
            action,
            data
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
}