using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDatabase _redisDb;

    public CacheService(IConfiguration configuration)
    {
        // Leemos la cadena de conexión de Redis (por defecto suele ser "localhost:6379")
        var connectionString = configuration["RedisSettings:ConnectionString"] ?? "localhost:6379";
        var redis = ConnectionMultiplexer.Connect(connectionString);
        _redisDb = redis.GetDatabase();
    }

    public async Task SetActiveSessionAsync(Guid userId, string deviceId)
    {
        // Estructura de la llave: spacia:session:{userId}
        string key = $"spacia:session:{userId}";

        // Objeto con el payload solicitado por el criterio de aceptación
        var sessionData = new
        {
            UserId = userId,
            DeviceId = deviceId,
            Timestamp = DateTime.UtcNow
        };

        string jsonPayload = JsonSerializer.Serialize(sessionData);

        // Guardamos en Redis con una expiración de 15 minutos (alineado al ciclo de vida del QR)
        await _redisDb.StringSetAsync(key, jsonPayload, TimeSpan.FromMinutes(15));
    }
}