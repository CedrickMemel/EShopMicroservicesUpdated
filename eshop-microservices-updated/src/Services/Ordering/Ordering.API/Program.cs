using IdentityService.Services.UserServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ordering.API;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Data.Extensions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//JWT bearer token
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // true in prod
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("OrderWriteAccess", policy =>
    {
        policy.RequireAssertion(ctx =>
        {
            var hasScope = ctx.User.HasClaim("scope", Scopes.OrderingRead);
            var hasRole = ctx.User.IsInRole("Admin") || ctx.User.IsInRole("Seller");
            return hasScope || hasRole;
        });
    }).AddPolicy("OrderReadAccess", policy =>
    {
        policy.RequireAssertion(ctx =>
        {
            var hasScope = ctx.User.HasClaim("scope", Scopes.OrderingRead);
            var hasRole = ctx.User.IsInRole("Admin") || ctx.User.IsInRole("Seller")
                          || ctx.User.IsInRole("Customer");
            return hasScope || hasRole;
        });
    });

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastrusctureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseApiServices();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}

app.Run();
