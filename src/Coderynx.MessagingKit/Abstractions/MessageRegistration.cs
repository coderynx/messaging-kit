namespace Coderynx.MessagingKit.Abstractions;

public sealed record MessageRegistration(Type MessageType, Type? ConsumerType, string? Topic = null)
{
    public string GetTopicName()
    {
        if (Topic is not null)
        {
            return Topic;
        }

        return string.Concat(MessageType.Name
            .Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + char.ToLower(x) : char.ToLower(x).ToString()));
    }
}