using StackFood.Customers.Application.DTOs;
using StackFood.Customers.Application.Interfaces;
using StackFood.Customers.Domain.Entities;
using StackFood.Customers.Domain.Events;

namespace StackFood.Customers.Application.UseCases;

public class CreateCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICognitoService _cognitoService;
    private readonly IEventPublisher _eventPublisher;

    public CreateCustomerUseCase(
        ICustomerRepository customerRepository,
        ICognitoService cognitoService,
        IEventPublisher eventPublisher)
    {
        _customerRepository = customerRepository;
        _cognitoService = cognitoService;
        _eventPublisher = eventPublisher;
    }

    public async Task<CustomerDTO> ExecuteAsync(CreateCustomerRequest request)
    {
        // Verificar se já existe um customer com o CPF
        var existingCustomer = await _customerRepository.GetByCpfAsync(request.Cpf);
        if (existingCustomer != null)
            throw new InvalidOperationException("Customer with this CPF already exists");

        // Verificar se já existe um customer com o Email
        var existingEmail = await _customerRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
            throw new InvalidOperationException("Customer with this email already exists");

        // Criar usuário no Cognito
        string cognitoUserId;
        try
        {
            cognitoUserId = await _cognitoService.CreateUserAsync(request.Cpf, request.Email, request.Name);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create user in Cognito: {ex.Message}", ex);
        }

        // Criar customer no banco de dados
        var customer = new Customer(request.Name, request.Email, request.Cpf, cognitoUserId);
        await _customerRepository.CreateAsync(customer);

        // Publicar evento (opcional, pode ser configurado via settings)
        try
        {
            var customerEvent = new CustomerCreatedEvent(
                customer.Id,
                customer.Name,
                customer.Email,
                customer.Cpf,
                customer.CognitoUserId,
                customer.CreatedAt
            );

            // O topicArn será injetado via configuração
            // await _eventPublisher.PublishAsync(customerEvent, topicArn);
        }
        catch
        {
            // Log do erro, mas não falha a operação
        }

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
