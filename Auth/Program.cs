using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers
builder.Services.AddControllers();

// 2. Global exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 3. Database
builder.Services.AddDbContext<AuthContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDb")));

// 4. Your classes, in the order the controller needs them
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();

builder.Services.AddHealthChecks();         // GET /health for docker compose and the gateway
builder.Services.AddOpenApi();          // you already had this;

var app = builder.Build();

app.UseExceptionHandler();              // 5. FIRST, so it can catch everything below it

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.MapControllers();                   // 6. connects [Route]/[HttpPost] to the pipeline
app.Run();