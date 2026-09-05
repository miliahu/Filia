namespace Filia.Application.Common.Interfaces;

public interface IDateTime
{
    DateTimeOffset UtcNow { get; }
}
