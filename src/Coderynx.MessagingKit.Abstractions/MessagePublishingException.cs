namespace Coderynx.MessagingKit.Abstractions;

public sealed class MessagePublishingException(string busName, string message, Exception innerException)
    : Exception(message, innerException)
{
    public string BusName { get; } = busName;
}