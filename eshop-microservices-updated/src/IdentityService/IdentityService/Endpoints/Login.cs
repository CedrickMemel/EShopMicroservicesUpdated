namespace IdentityService.Endpoints;

public class Login : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/account/login", async (LoginRequest dto,
            UserManager<IdentityUser> userManager, IJwtTokenService jwt) =>
        {
            var user = await userManager.FindByNameAsync(dto.UserName) ?? await userManager.FindByEmailAsync(dto.UserName);
            if (user == null) return Results.Unauthorized();

            var valid = await userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid) return Results.Unauthorized();

            var roles = (await userManager.GetRolesAsync(user)).ToArray();
            var token = await jwt.GenerateTokenAsync(user, roles);
            return Results.Ok(new AuthResponse(token));
        })
        .WithName("Login")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
