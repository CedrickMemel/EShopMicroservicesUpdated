namespace IdentityService.Services;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(IdentityUser user, string[] roles);
    string GenerateClientToken(string clientId, string[] scopes);
}
