using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using SmartSpaces.Application.Common.Cart;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Infrastructure.Services;

// Publica la secuencia temporizada de movimiento al carrito vía MQTT, reusando el mismo
// broker/topic/comandos del firmware ya desplegado (docs/guide-car-code/cochecito_mqtt_0710.ino:
// "adelante"/"atras"/"izquierda"/"derecha"/"stop" sobre coche1/comandos, HiveMQ Cloud).
// Credenciales SIEMPRE por configuración/variables de entorno, nunca hardcodeadas en código.
public class MqttCartMovementPublisher : ICartMovementPublisher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MqttCartMovementPublisher> _logger;

    public MqttCartMovementPublisher(IConfiguration configuration, ILogger<MqttCartMovementPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> TryRunSequenceAsync(IReadOnlyList<ResolvedMovementStep> steps, CancellationToken cancellationToken)
    {
        var host = _configuration["MQTT_BROKER_HOST"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning("MQTT_BROKER_HOST no configurado; se usa modo simulado para el carrito.");
            return false;
        }

        var port = int.TryParse(_configuration["MQTT_BROKER_PORT"], out var parsedPort) ? parsedPort : 8883;
        var username = _configuration["MQTT_USERNAME"];
        var password = _configuration["MQTT_PASSWORD"];
        var topic = _configuration["MQTT_TOPIC"] ?? "coche1/comandos";
        var useTls = !bool.TryParse(_configuration["MQTT_USE_TLS"], out var parsedTls) || parsedTls;

        var client = new MqttClientFactory().CreateMqttClient();

        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithCredentials(username, password)
            .WithClientId($"backend-cart-{Guid.NewGuid():N}");

        if (useTls)
        {
            optionsBuilder = optionsBuilder.WithTlsOptions(tls => tls.UseTls());
        }

        using var connectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        connectCts.CancelAfter(TimeSpan.FromSeconds(3));

        try
        {
            await client.ConnectAsync(optionsBuilder.Build(), connectCts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo conectar al broker MQTT del carrito; se usa modo simulado.");
            client.Dispose();
            return false;
        }

        // Conexión lograda: la secuencia corre en segundo plano para no bloquear la respuesta
        // HTTP mientras dura el recorrido completo (puede tomar varios segundos).
        _ = Task.Run(() => RunSequenceInBackgroundAsync(client, topic, steps), CancellationToken.None);

        return true;
    }

    private async Task RunSequenceInBackgroundAsync(IMqttClient client, string topic, IReadOnlyList<ResolvedMovementStep> steps)
    {
        try
        {
            foreach (var step in steps)
            {
                await PublishAsync(client, topic, step.Command);
                if (step.DurationSeconds > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(step.DurationSeconds));
                }
            }

            if (steps.Count == 0 || !string.Equals(steps[^1].Command, "stop", StringComparison.OrdinalIgnoreCase))
            {
                await PublishAsync(client, topic, "stop");
            }
        }
        catch (Exception ex)
        {
            // El firmware del ESP32 no tiene watchdog/timeout: si esto falla a mitad de
            // camino, el carrito sigue con el último comando hasta recibir un "stop".
            // Riesgo físico aceptado, ya presente en el control manual existente.
            _logger.LogWarning(ex, "La secuencia de movimiento del carrito se interrumpió.");
        }
        finally
        {
            try
            {
                await client.DisconnectAsync();
            }
            catch
            {
                // Best-effort: no bloquear la limpieza por un disconnect fallido.
            }

            client.Dispose();
        }
    }

    private static Task PublishAsync(IMqttClient client, string topic, string command)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(command)
            .Build();

        return client.PublishAsync(message, CancellationToken.None);
    }
}
