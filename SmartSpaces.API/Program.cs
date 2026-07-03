using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartSpaces.API.Exceptions;
using SmartSpaces.Application;
using SmartSpaces.Application.Common.Interfaces;
using SmartSpaces.Infrastructure.Persistence;
using SmartSpaces.Infrastructure.Security;
using SmartSpaces.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddSingleton<ICacheService, CacheService>(); // Usamos Singleton para mantener viva la conexión a Redis|
builder.Services.AddApplicationServices();
builder.Services.AddControllers();

// Cliente HTTP tipado hacia el microservicio RAG (FastAPI)
var ragBaseUrl = builder.Configuration["RAG_BASE_URL"] ?? throw new InvalidOperationException("RAG_BASE_URL no configurado.");
builder.Services.AddHttpClient<IRagService, RagHttpService>(client =>
{
    client.BaseAddress = new Uri(ragBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
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

app.UseExceptionHandler(new ExceptionHandlerOptions());

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartSpaces API V1"));

app.UseHttpsRedirection();
app.UseCors("Frontends");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();