using StackFood.Customers.Application.DTOs;
using StackFood.Customers.Application.Interfaces;

namespace StackFood.Customers.Application.UseCases;

public class GetCustomerByCpfUseCase
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByCpfUseCase(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDTO?> ExecuteAsync(string cpf)
    {
        var customer = await _customerRepository.GetByCpfAsync(cpf);
        if (customer == null)
            return null;

        return new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Cpf = customer.Cpf,
            CognitoUserId = customer.CognitoUserId,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}
