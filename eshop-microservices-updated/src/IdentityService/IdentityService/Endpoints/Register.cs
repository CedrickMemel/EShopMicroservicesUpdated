namespace IdentityService.Endpoints;

public class Register : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/account/register", async (RegisterRequest dto,
            UserManager<IdentityUser> userManager, IJwtTokenService jwt) =>
        {

            var exists = await userManager.FindByEmailAsync(dto.Email);
            if (exists != null) return Results.BadRequest(new { error = "Email already registered." });

            var user = new IdentityUser { UserName = dto.UserName, Email = dto.Email };
            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return Results.BadRequest(result.Errors.Select(e => e.Description));
            }

            // Assign default role if needed
            await userManager.AddToRoleAsync(user, "User");

            var token = await jwt.GenerateTokenAsync(user, ["User"]);
            return Results.Ok(new AuthResponse(token));
        })
        .WithName("Register")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
