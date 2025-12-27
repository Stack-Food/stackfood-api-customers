using StackFood.Customers.Domain.Entities;

namespace StackFood.Customers.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task<Customer?> GetByCpfAsync(string cpf);
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer?> GetByCognitoUserIdAsync(string cognitoUserId);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer> CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Guid id);
}
