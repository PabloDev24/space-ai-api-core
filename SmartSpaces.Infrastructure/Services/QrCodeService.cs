using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Infrastructure.Security;

public class QrCodeService : IQrCodeService
{
    private readonly IConfiguration _configuration;

    public QrCodeService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateEncryptedQrToken(Guid userId, string role)
    {
        // Usamos la misma secret key del JWT o una dedicada para el QR
        var secretKey = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException();
        var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(secretKey)); // Aseguramos 256 bits

        // Definimos el contenido interno del QR y calculamos su expiración (15 minutos)
        var qrPayload = new
        {
            Sub = userId,
            Role = role,
            Exp = DateTime.UtcNow.AddMinutes(15)        };

        var plainText = JsonSerializer.Serialize(qrPayload);

        // Encriptación AES
        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.GenerateIV(); // Vector de inicialización aleatorio para que cada QR sea único

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();

        // Escribimos el IV al inicio del stream para que el validador del torniquete/lector pueda leerlo
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        // Devolvemos una cadena segura para URLs y QR
        return Convert.ToBase64String(ms.ToArray())
            .Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}