using RabbitMQ.Client;

namespace Coderynx.MessagingKit.Transports.RabbitMq;

public static class LetterExtensions
{
    public static Letter ToLetter(this IReadOnlyBasicProperties properties, ReadOnlyMemory<byte> payload)
    {
        var headers = properties.Headers?.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString()) ?? [];

        var format = properties.ContentType switch
        {
            "application/json" => PayloadFormat.Json,
            "application/octet-stream" => PayloadFormat.Binary,
            "application/unknown" or "" => PayloadFormat.Unknown,
            null => PayloadFormat.Unknown,
            _ => throw new ArgumentOutOfRangeException()
        };

        var id = Guid.TryParse(properties.MessageId, out var parsedId) ? parsedId : Guid.Empty;

        var correlationId = Guid.TryParse(properties.CorrelationId, out var parsedCorrelationId)
            ? parsedCorrelationId
            : Guid.Empty;

        var publishedOn = properties.Timestamp is { UnixTime: var timestamp }
            ? DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime
            : DateTime.UtcNow;

        var payloadType = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name.Equals(properties.Type));

        if (payloadType is null)
        {
            throw new InvalidOperationException($"Type {properties.Type} not found");
        }

        return new Letter(
            id,
            correlationId,
            publishedOn,
            headers,
            format,
            payloadType,
            payload.ToArray()
        );
    }

    public static BasicProperties ToBasicProperties(this Letter letter)
    {
        var contentType = letter.PayloadFormat switch
        {
            PayloadFormat.Unknown => "application/unknown",
            PayloadFormat.Json => "application/json",
            PayloadFormat.Binary => "application/octet-stream",
            _ => throw new ArgumentOutOfRangeException()
        };

        return new BasicProperties
        {
            ContentType = contentType,
            ContentEncoding = "utf-8",
            Headers = letter.Headers.ToDictionary(x => x.Key, object? (x) => x.Value),
            DeliveryMode = DeliveryModes.Persistent,
            Priority = 0,
            CorrelationId = letter.CorrelationId.ToString(),
            ReplyTo = null,
            Expiration = null,
            MessageId = letter.Id.ToString(),
            Timestamp = new AmqpTimestamp(((DateTimeOffset)letter.PublishedOn).ToUnixTimeSeconds()),
            Type = letter.PayloadType.Name,
            UserId = null,
            AppId = null,
            ClusterId = null,
            Persistent = true,
            ReplyToAddress = null
        };
    }

    public static string GetTopicName(this Letter letter)
    {
        return string.Concat(letter.PayloadType.Name
            .Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + char.ToLower(x) : char.ToLower(x).ToString()));
    }
}