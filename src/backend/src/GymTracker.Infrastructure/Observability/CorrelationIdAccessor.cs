using System.Threading;

namespace GymTracker.Infrastructure.Observability;

public sealed class CorrelationIdAccessor
{
    private static readonly AsyncLocal<string?> CurrentCorrelationId = new();

    public string? Get()
    {
        return CurrentCorrelationId.Value;
    }

    public void Set(string correlationId)
    {
        CurrentCorrelationId.Value = correlationId;
    }
}
