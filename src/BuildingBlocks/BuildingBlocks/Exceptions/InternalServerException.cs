namespace BuildingBlocks.Exceptions;

public class InternalServerException(string message) : Exception(message)
{
    public InternalServerException(string message, string details) : this(message)
    {
        Details = details;
    }

    public string? Details { get; }
}
