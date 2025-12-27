using StackFood.Customers.Application.DTOs;
using StackFood.Customers.Application.Interfaces;

namespace StackFood.Customers.Application.UseCases;

public class AuthenticateCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICognitoService _cognitoService;

    public AuthenticateCustomerUseCase(
        ICustomerRepository customerRepository,
        ICognitoService cognitoService)
    {
        _customerRepository = customerRepository;
        _cognitoService = cognitoService;
    }

    public async Task<AuthenticateResponse> ExecuteAsync(AuthenticateRequest request)
    {
        // Se CPF vazio, autenticar como convidado
        if (string.IsNullOrWhiteSpace(request.Cpf))
        {
            var guestToken = await _cognitoService.AuthenticateGuestAsync();
            return new AuthenticateResponse
            {
                Token = guestToken,
                Customer = null!,
                Message = "Authenticated as guest"
            };
        }

        // Buscar customer no banco
        var customer = await _customerRepository.GetByCpfAsync(request.Cpf);
        if (customer == null)
            throw new UnauthorizedAccessException("Customer not found");

        if (!customer.IsActive)
            throw new UnauthorizedAccessException("Customer is inactive");

        // Autenticar no Cognito
        string token;
        try
        {
            token = await _cognitoService.AuthenticateAsync(request.Cpf);
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException($"Authentication failed: {ex.Message}", ex);
        }

        return new AuthenticateResponse
        {
            Token = token,
            Customer = new CustomerDTO
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Cpf = customer.Cpf,
                CognitoUserId = customer.CognitoUserId,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            },
            Message = "Customer authenticated successfully"
        };
    }
}
