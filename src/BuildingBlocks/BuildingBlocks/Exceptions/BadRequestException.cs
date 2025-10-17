namespace BuildingBlocks.Exceptions;

public class BadRequestException(string message) : Exception(message)
{
    public BadRequestException(string message, string details) : this(message)
    {
        Details = details;
    }

    public string? Details { get; }
}
