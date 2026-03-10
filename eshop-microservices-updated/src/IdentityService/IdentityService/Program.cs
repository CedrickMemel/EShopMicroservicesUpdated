using IdentityService.Services.IdentityTokenServices;
using IdentityService.Services.JwtServices;
using IdentityService.Services.UserServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// ---- EF + Identity ----
builder.Services.AddDbContext<UserDbContext>(opts =>
    opts.UseSqlServer(configuration.GetConnectionString("IdentityDatabase")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(opts =>
{
    opts.Password.RequireDigit = true;
    opts.Password.RequiredLength = 6;
    opts.Password.RequireNonAlphanumeric = false;
    opts.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<UserDbContext>()
.AddDefaultTokenProviders();

// ---- JWT settings ----
var jwtSection = configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // true en prod
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSection["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

// ---- Services ----
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddHttpClient<IIdentityTokenClient, IdentityTokenClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["Identity:BaseUrl"]!);
});

builder.WebHost.ConfigureKestrel((context, options) =>
{
    var certPath = context.Configuration["Kestrel:Certificates:Default:Path"];
    var certPassword = context.Configuration["Kestrel:Certificates:Default:Password"];

    options.ListenAnyIP(6065, listenOptions =>
    {
        listenOptions.UseHttps(certPath!, certPassword);
    });
});
var app = builder.Build();

// Apply migrations automatically (convenience in dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    db.Database.Migrate();
}

// Middleware
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
