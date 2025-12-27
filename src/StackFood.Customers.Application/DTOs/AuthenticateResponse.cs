namespace StackFood.Customers.Application.DTOs;

public class AuthenticateResponse
{
    public string Token { get; set; } = string.Empty;
    public CustomerDTO Customer { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
}
