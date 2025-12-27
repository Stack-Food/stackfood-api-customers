namespace StackFood.Customers.Domain.Events;

public class CustomerUpdatedEvent
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public DateTime Timestamp { get; set; }

    public CustomerUpdatedEvent()
    {
        Timestamp = DateTime.UtcNow;
    }

    public CustomerUpdatedEvent(Guid customerId, string name, string email, DateTime updatedAt)
    {
        CustomerId = customerId;
        Name = name;
        Email = email;
        UpdatedAt = updatedAt;
        Timestamp = DateTime.UtcNow;
    }
}
