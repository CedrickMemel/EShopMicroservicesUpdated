using Catalog.API;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehaviors<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
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
var app = builder.Build();

//Configure the HTTP request pipeline.
app.MapCarter();
app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
app.UseExceptionHandler(opt => { });
app.Run();
