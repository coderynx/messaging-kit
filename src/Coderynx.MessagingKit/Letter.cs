using System.Text.Json;

namespace Coderynx.MessagingKit;

public sealed record Letter
{
    public Letter(
        Guid id,
        Guid correlationId,
        DateTime publishedOn,
        Dictionary<string, string?> headers,
        PayloadFormat payloadFormat,
        Type payloadType,
        byte[] payload)
    {
        Id = id;
        CorrelationId = correlationId;
        PublishedOn = publishedOn;
        Headers = headers;
        PayloadFormat = payloadFormat;
        PayloadType = payloadType;
        Payload = payload;
    }

    private Letter(Dictionary<string, string?> headers, PayloadFormat payloadFormat, Type messageType, byte[] payload)
    {
        Id = Guid.CreateVersion7();
        CorrelationId = Guid.CreateVersion7();
        PublishedOn = DateTime.UtcNow;
        Headers = headers;
        PayloadFormat = payloadFormat;
        PayloadType = messageType;
        Payload = payload;
    }

    public Guid Id { get; }
    public Guid CorrelationId { get; }
    public DateTime PublishedOn { get; }
    public IReadOnlyDictionary<string, string?> Headers { get; }
    public PayloadFormat PayloadFormat { get; }
    public Type PayloadType { get; }
    public byte[] Payload { get; }

    public static Letter Create<TPayload>(
        Dictionary<string, string?> headers,
        PayloadFormat payloadFormat,
        TPayload payload) where TPayload : class
    {
        var encodedPayload = JsonSerializer.SerializeToUtf8Bytes(payload);

        return new Letter(headers, payloadFormat, typeof(TPayload), encodedPayload);
    }
}