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

            switch (request.Action)
            {
                case "getGames":
                    await GetGames(web);
                    break;
                default:
                    MessageBox.Show($"Unknown action: {request.Action}");
                    break;
            }
        };
    }

    private static async Task GetGames(WebView2 web)
    {
        await Send(web, "getGames", Game.GetGames());
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
    }
}