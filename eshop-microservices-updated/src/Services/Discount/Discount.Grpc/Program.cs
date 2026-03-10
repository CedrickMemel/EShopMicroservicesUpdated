using Discount.Grpc.Data;
using Discount.Grpc.Services;

//using Discount.Grpc.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDbContext<DiscountContext>(opts =>
{
    opts.UseSqlite(builder.Configuration.GetConnectionString("Database"));
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
TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMigration();
app.MapGrpcService<DiscountService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
