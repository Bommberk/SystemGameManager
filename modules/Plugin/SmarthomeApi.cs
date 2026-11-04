namespace SystemGameManager.Plugin;

class SmarthomeApi
{
    private readonly string _apiUrl;
    private readonly string _apiKey;

    public SmarthomeApi()
    {
        _apiUrl = GlobalConfig.Settings.SmarthomeApiConfig.ApiUrl;
        _apiKey = GlobalConfig.Settings.SmarthomeApiConfig.ApiKey;
    }

    public async Task<string> SendDeviceInfoAsync()
    {
        var pcInfo = new PcInfoController();
        var publisherUuid = pcInfo.PublisherUUID;
        if(string.IsNullOrEmpty(publisherUuid))
        {
            throw new InvalidOperationException("Publisher UUID is null or empty.");
        }
        HttpClient client = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_apiUrl}/receiveSystemGameManagerData?device_id={publisherUuid}"
        );
        request.Headers.Add("Api-Key", _apiKey);
        var response = await client.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }
}