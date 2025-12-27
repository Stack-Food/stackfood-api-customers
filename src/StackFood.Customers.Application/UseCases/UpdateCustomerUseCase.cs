using StackFood.Customers.Application.DTOs;
using StackFood.Customers.Application.Interfaces;
using StackFood.Customers.Domain.Events;

namespace StackFood.Customers.Application.UseCases;

public class UpdateCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IEventPublisher _eventPublisher;

    public UpdateCustomerUseCase(
        ICustomerRepository customerRepository,
        IEventPublisher eventPublisher)
    {
        _customerRepository = customerRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<CustomerDTO> ExecuteAsync(Guid id, UpdateCustomerRequest request)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            throw new InvalidOperationException("Customer not found");

        customer.Update(request.Name, request.Email);
        await _customerRepository.UpdateAsync(customer);

        // Publicar evento
        try
        {
            var customerEvent = new CustomerUpdatedEvent(
                customer.Id,
                customer.Name,
                customer.Email,
                customer.UpdatedAt
            );

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
