using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Auth.service;
using NLog.Web;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //Controllers
        builder.Services.AddControllers();

        //Redis
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration["redis:ConnectionString"]));
        //Global exception handling
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        //Database
        builder.Services.AddDbContext<AuthContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDb")));

        //Your classes, in the order the controller needs them
        builder.Services.AddScoped<IUserRepo, UserRepo>();
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<TokenService>();

        //nlog 
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        //openTelemmetry
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("auth"))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter());

        builder.Logging.AddOpenTelemetry(o => o.AddOtlpExporter());

        builder.Services.AddHealthChecks();      // GET /health for docker compose and the gateway
        builder.Services.AddOpenApi();          //  you already had this;

        var app = builder.Build();

        app.UseExceptionHandler();              // 5. FIRST, so it can catch everything below it

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapHealthChecks("/health");
        app.MapControllers();                   // 6. connects [Route]/[HttpPost] to the pipeline
        app.Run();
    }
}