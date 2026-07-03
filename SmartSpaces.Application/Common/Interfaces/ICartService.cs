namespace SmartSpaces.Application.Common.Interfaces;

public record CartStatus(
    string Id,
    string Status,
    string Mode,
    int Battery,
    string CurrentZone,
    bool IsSimulated
);

public record CartRoute(
    string From,
    string To,
    int EstimatedTimeMinutes,
    IReadOnlyList<string> Steps,
    bool IsSimulated
);

public interface ICartService
{
    CartStatus GetStatus();
    CartRoute BuildRoute(string from, string to);
}
