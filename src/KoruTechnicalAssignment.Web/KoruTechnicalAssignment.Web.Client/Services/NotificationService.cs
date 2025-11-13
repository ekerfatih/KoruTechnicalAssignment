using System.Collections.Concurrent;

namespace KoruTechnicalAssignment.Web.Client.Services;

public sealed class NotificationService
{
    public event Action<NotificationMessage>? OnNotify;

    public void Success(string message) => Notify(NotificationLevel.Success, message);

    public void Error(string message) => Notify(NotificationLevel.Error, message);

    public void Info(string message) => Notify(NotificationLevel.Info, message);

    private void Notify(NotificationLevel level, string message)
    {
        OnNotify?.Invoke(new NotificationMessage(level, message, DateTime.UtcNow));
    }
}

public enum NotificationLevel
{
    Info,
    Success,
    Error
}

public sealed record NotificationMessage(NotificationLevel Level, string Message, DateTime Timestamp);
