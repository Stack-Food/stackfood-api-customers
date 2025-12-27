using FluentAssertions;
using Moq;
using StackFood.Customers.Application.DTOs;
using StackFood.Customers.Application.Interfaces;
using StackFood.Customers.Application.UseCases;
using StackFood.Customers.Domain.Entities;

namespace StackFood.Customers.Tests.UseCases;

public class UpdateCustomerUseCaseTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly UpdateCustomerUseCase _useCase;

    public UpdateCustomerUseCaseTests()
    {
        _repositoryMock = new Mock<ICustomerRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _useCase = new UpdateCustomerUseCase(_repositoryMock.Object, _eventPublisherMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldUpdateCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer("John Doe", "john@example.com", "12345678900");
        var request = new UpdateCustomerRequest
        {
            Name = "Jane Doe",
            Email = "jane@example.com"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Customer>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(customerId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Email.Should().Be(request.Email);
        customer.Name.Should().Be(request.Name);
        customer.Email.Should().Be(request.Email);

        _repositoryMock.Verify(r => r.UpdateAsync(customer), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentCustomer_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new UpdateCustomerRequest
        {
            Name = "Jane Doe",
            Email = "jane@example.com"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var act = () => _useCase.ExecuteAsync(customerId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Customer not found*");

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
    }
}
