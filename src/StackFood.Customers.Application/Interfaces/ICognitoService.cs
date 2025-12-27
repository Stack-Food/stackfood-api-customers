namespace StackFood.Customers.Application.Interfaces;

public interface ICognitoService
{
    Task<string> CreateUserAsync(string cpf, string email, string name);
    Task<string> AuthenticateAsync(string cpf);
    Task<string> AuthenticateGuestAsync();
    Task DeleteUserAsync(string cpf);
}
