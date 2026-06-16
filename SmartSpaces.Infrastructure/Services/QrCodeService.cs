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

    public QrValidationResult? ValidateAndDecryptQrToken(string encryptedToken)
    {
        try
        {
            var secretKey = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException();
            var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(secretKey));

            // Reconstruimos los bytes desde la cadena Base64 segura
            string base64 = encryptedToken.Replace("-", "+").Replace("_", "/");
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            var fullBytes = Convert.FromBase64String(base64);

            using var aes = Aes.Create();
            aes.Key = keyBytes;

            // Extraemos el IV del inicio del array de bytes (son los primeros 16 bytes)
            var iv = new byte[aes.BlockSize / 8];
            Array.Copy(fullBytes, 0, iv, 0, iv.Length);
            aes.IV = iv;

            // El resto de los bytes son el texto cifrado real
            var cipherTextBytes = new byte[fullBytes.Length - iv.Length];
            Array.Copy(fullBytes, iv.Length, cipherTextBytes, 0, cipherTextBytes.Length);

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipherTextBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            var plainText = sr.ReadToEnd();

            // Deserializamos el JSON interno del QR
            var payload = JsonSerializer.Deserialize<QrPayloadDto>(plainText);

            if (payload == null) return null;

            return new QrValidationResult(payload.Sub, payload.Role, payload.Exp);
        }
        catch
        {
            // Si la firma es incorrecta, fue manipulado o el Base64 se rompió, truena la desencriptación y retornamos null
            return null;
        }
    }
    private class QrPayloadDto
    {
        public Guid Sub { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime Exp { get; set; }
    }
}