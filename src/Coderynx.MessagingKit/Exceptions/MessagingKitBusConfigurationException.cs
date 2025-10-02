namespace Coderynx.MessagingKit.Exceptions;

public sealed class MessagingKitBusConfigurationException(string parameterName) :
    Exception($"Configuration parameter '{parameterName}' is invalid or missing.")
{
    public string ParameterName { get; } = parameterName;
}