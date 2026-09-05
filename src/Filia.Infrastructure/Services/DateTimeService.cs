using Filia.Application.Common.Interfaces;

namespace Filia.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
