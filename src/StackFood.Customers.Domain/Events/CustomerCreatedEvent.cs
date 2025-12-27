namespace StackFood.Customers.Domain.Events;

public class CustomerCreatedEvent
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? CognitoUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime Timestamp { get; set; }

    public CustomerCreatedEvent()
    {
        Timestamp = DateTime.UtcNow;
    }

    public CustomerCreatedEvent(Guid customerId, string name, string email, string cpf, string? cognitoUserId, DateTime createdAt)
    {
        CustomerId = customerId;
        Name = name;
        Email = email;
        Cpf = cpf;
        CognitoUserId = cognitoUserId;
        CreatedAt = createdAt;
        Timestamp = DateTime.UtcNow;
    }
}
