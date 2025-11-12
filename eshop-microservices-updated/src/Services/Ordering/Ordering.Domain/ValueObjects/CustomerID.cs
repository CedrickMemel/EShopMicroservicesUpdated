namespace Ordering.Domain.ValueObjects;

public record CustomerID
{
    public Guid Value { get; }
    private CustomerID(Guid value) => Value = value;
    public static CustomerID Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new DomainException("CustomerId cannot be empty.");
        }

        return new CustomerID(value);
    }
}
