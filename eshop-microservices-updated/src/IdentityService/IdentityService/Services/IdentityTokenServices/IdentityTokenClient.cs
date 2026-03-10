namespace IdentityService.Services.IdentityTokenServices;

public class IdentityTokenClient(HttpClient httpClient, IConfiguration config) : IIdentityTokenClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IConfiguration _config = config;

    public async Task<string> GetTokenAsync()
    {
        var clientId = _config["Identity:ClientId"];
        var clientSecret = _config["Identity:ClientSecret"];

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId!,
            ["client_secret"] = clientSecret!
        });

        var response = await _httpClient.PostAsync("/connect/token", content);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<TokenResponse>();

        return json!.Access_token;
    }
}

public record TokenResponse(string Access_token);

