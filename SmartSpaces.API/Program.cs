using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SmartSpaces.API.Exceptions;
using SmartSpaces.Application;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Application.Common.Options;
using SmartSpaces.Infrastructure.Persistence;
using SmartSpaces.Infrastructure.Security;
using SmartSpaces.Infrastructure.Services;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Logger estructurado: consola (siempre) + archivo rotado diario (14 días) en logs/.
// Reemplaza el logging por defecto — el resto del código no cambia, sigue usando
// ILogger<T> normal (AzureSpeechSynthesizer, MqttCartMovementPublisher, etc.).
builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/spaceia-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14));

// Catálogo de destinos del carrito (navegación guiada por voz). reloadOnChange:true permite
// agregar/editar rutas sin recompilar (docs/00 §3.4).
builder.Configuration.AddJsonFile("cart-routes.json", optional: false, reloadOnChange: true);
builder.Services.Configure<CartRoutesOptions>(builder.Configuration.GetSection("CartRoutes"));

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<ICartService, SimulatedCartService>(); // Carrito simulado (sin hardware, docs/00 §3.4)
builder.Services.AddScoped<ICartMovementPublisher, MqttCartMovementPublisher>(); // MQTT real hacia el ESP32 del carrito
builder.Services.AddSingleton<ICartLastRouteTracker, InMemoryCartLastRouteTracker>(); // Un solo carrito físico: estado en memoria
builder.Services.AddSingleton<ICacheService, CacheService>(); // Usamos Singleton para mantener viva la conexión a Redis|
builder.Services.AddScoped<IDocumentStorage, LocalDocumentStorage>(); // Documentos base del RAG (disco local en dev, Blob Storage en Azure)
builder.Services.AddApplicationServices();
builder.Services.AddControllers();

// Cliente HTTP tipado hacia el microservicio RAG (FastAPI)
var ragBaseUrl = builder.Configuration["RAG_BASE_URL"] ?? throw new InvalidOperationException("RAG_BASE_URL no configurado.");
builder.Services.AddHttpClient<IRagService, RagHttpService>(client =>
{
    client.BaseAddress = new Uri(ragBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

// Cliente HTTP tipado hacia Azure Speech (voz IA, uso manual/opcional desde el front).
// Si AZURE_SPEECH_REGION no está configurado, BaseAddress queda null y el servicio
// responde Success:false sin intentar la llamada (ver AzureSpeechSynthesizer).
var azureSpeechRegion = builder.Configuration["AZURE_SPEECH_REGION"];
builder.Services.AddHttpClient<ISpeechSynthesisService, AzureSpeechSynthesizer>(client =>
{
    if (!string.IsNullOrWhiteSpace(azureSpeechRegion))
    {
        client.BaseAddress = new Uri($"https://{azureSpeechRegion}.tts.speech.microsoft.com/");
    }
    client.Timeout = TimeSpan.FromSeconds(10);
});

// CONFIGURACIÓN DE AUTHENTICATION CON JWT BEARER
var secretKey = builder.Configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Elimina el retraso de tolerancia por defecto de 5 min
    };
});

//Metodo de conexion a PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Límite por IP en /api/cart/* (voice y speech son AllowAnonymous: sin esto, cualquiera
// con la URL puede saturar el RAG/MQTT o gastar la cuota de Azure Speech sin login de por medio).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"error\":\"Demasiadas solicitudes, intenta de nuevo en un momento.\"}", cancellationToken);
    };

    options.AddPolicy("cart", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontends", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging(); // log estructurado por request: método, ruta, status, duración

app.UseExceptionHandler(new ExceptionHandlerOptions());

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartSpaces API V1"));

app.UseHttpsRedirection();
app.UseCors("Frontends");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush(); // asegura que el sink de archivo vacíe el buffer al apagar
}