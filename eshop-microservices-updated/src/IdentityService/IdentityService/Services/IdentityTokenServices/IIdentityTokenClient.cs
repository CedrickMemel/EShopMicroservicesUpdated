namespace IdentityService.Services.IdentityTokenServices;

public interface IIdentityTokenClient
{
    Task<string> GetTokenAsync();
}
