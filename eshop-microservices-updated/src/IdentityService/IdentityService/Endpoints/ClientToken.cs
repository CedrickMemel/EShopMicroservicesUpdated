
namespace IdentityService.Endpoints;

public class ClientToken : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/connect/token",
            async (HttpRequest request, IJwtTokenService jwt, IConfiguration config) =>
            {
                if (!request.HasFormContentType) return Results.BadRequest(new { error = "invalid_request" });

                var form = await request.ReadFormAsync();
                var clientId = form["client_id"].ToString();
                var clientSecret = form["client_secret"].ToString();

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                    return Results.BadRequest(new { error = "invalid_request" });

                var clientsSection = config.GetSection("Clients");
                var clientSection = clientsSection.GetSection(clientId);
                if (!clientSection.Exists()) return Results.BadRequest(new { error = "invalid_client" });

                var secret = clientSection["secret"];
                if (secret != clientSecret) return Results.BadRequest(new { error = "invalid_client" });

                var scopes = clientSection["scopes"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                var token = jwt.GenerateClientToken(clientId, scopes);
                return Results.Ok(new { access_token = token, token_type = "Bearer", expires_in = 3600 });
            })
            .WithName("ClientToken")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
