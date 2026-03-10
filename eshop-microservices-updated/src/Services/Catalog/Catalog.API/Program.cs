using Catalog.API;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehaviors<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

//Authentication & Authorization
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
    .AddPolicy("CatalogWriteAccess", policy =>
    {
        policy.RequireAssertion(ctx =>
        {
            var hasScope = ctx.User.HasClaim("scope", Scopes.CatalogWrite);
            var hasRole = ctx.User.IsInRole("Admin") || ctx.User.IsInRole("Seller");
            return hasScope || hasRole;
        });
    }).AddPolicy("CatalogReadAccess", policy =>
    {
        policy.RequireAssertion(ctx =>
        {
            var hasScope = ctx.User.HasClaim("scope", Scopes.CatalogRead);
            var hasRole = ctx.User.IsInRole("Admin") || ctx.User.IsInRole("Seller")
                          || ctx.User.IsInRole("Customer");
            return hasScope || hasRole;
        });
    });

builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddCarter();
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions();

if (builder.Environment.IsDevelopment())
{
    builder.Services.InitializeMartenWith<CatalogInitialData>();
}

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!);

builder.WebHost.ConfigureKestrel((context, options) =>
{
    var certPath = context.Configuration["Kestrel:Certificates:Default:Path"];
    var certPassword = context.Configuration["Kestrel:Certificates:Default:Password"];

    options.ListenAnyIP(5050, listenOptions =>
    {
        listenOptions.UseHttps(certPath!, certPassword);
    });
});
var app = builder.Build();

//Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();
app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
app.UseExceptionHandler(opt => { });
app.Run();
