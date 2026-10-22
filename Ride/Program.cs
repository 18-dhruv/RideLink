using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers
builder.Services.AddControllers();

//nlog
builder.Logging.ClearProviders();
builder.Host.UseNLog();

//openTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("ride"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

builder.Logging.AddOpenTelemetry(o => o.AddOtlpExporter());

// 2. Global exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 3. Database (RideDb, this service's own database)
builder.Services.AddDbContext<RideContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("RideDb")));

// 4. JWT: Ride never calls Auth. It checks the token Auth signed using the same
//    Key, Issuer and Audience. The key comes from the env var Jwt__Key.
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("Jwt:Key is missing, set the Jwt__Key environment variable");
}
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// 5. Your classes
builder.Services.AddScoped<IRideRepo, RideRepo>();
builder.Services.AddScoped<RideLifecycleService>();   
builder.Services.AddScoped<RideCreationService>();
builder.Services.AddScoped<RideQueryService>();

// 6. Health endpoint for docker compose and the gateway
builder.Services.AddHealthChecks();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();              // first, so it catches everything below it

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();                // reads the Bearer token into User
app.UseAuthorization();                 // enforces [Authorize] / [Authorize(Roles = ...)]

app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
