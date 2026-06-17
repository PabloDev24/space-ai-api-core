using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSpaces.Application.Common.Interfaces
{
    public interface IQrCodeService
    {
        string GenerateEncryptedQrToken(Guid userId, string role);

        QrValidationResult? ValidateAndDecryptQrToken(string qrToken);
    }

    public record QrValidationResult(Guid UserId, string Role, DateTime ExpiredAt);
}
