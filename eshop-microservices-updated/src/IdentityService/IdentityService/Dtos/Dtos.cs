namespace IdentityService.Dtos;

public record RegisterRequest(string UserName, string Email, string Password);
public record LoginRequest(string UserName, string Password);
public record AuthResponse(string AccessToken);
public record ClientTokenRequest(string client_id, string client_secret);
