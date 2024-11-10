using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors(p =>
{
    p.AddPolicy("Cors", op =>
    {
        op.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Load Ocelot configuration
builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add Ocelot services
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// Use CORS before Ocelot
app.UseCors("Cors");

// Use Ocelot after CORS middleware
await app.UseOcelot();

app.Run();
