namespace SmartSpaces.Application.Common.Interfaces;

public interface ICacheService
{
    Task SetActiveSessionAsync(Guid userId, string deviceId);
}