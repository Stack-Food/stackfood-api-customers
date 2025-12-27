using FluentAssertions;
using Moq;
using StackFood.Customers.Application.Interfaces;
using StackFood.Customers.Application.UseCases;
using StackFood.Customers.Domain.Entities;

namespace StackFood.Customers.Tests.UseCases;

public class GetCustomerByIdUseCaseTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock;
    private readonly GetCustomerByIdUseCase _useCase;

    public GetCustomerByIdUseCaseTests()
    {
        _repositoryMock = new Mock<ICustomerRepository>();
        _useCase = new GetCustomerByIdUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingCustomer_ShouldReturnCustomerDTO()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer("John Doe", "john@example.com", "12345678900", "cognito-123");
        _repositoryMock.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _useCase.ExecuteAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.Name.Should().Be(customer.Name);
        result.Email.Should().Be(customer.Email);
        result.Cpf.Should().Be(customer.Cpf);
        result.CognitoUserId.Should().Be(customer.CognitoUserId);
        result.IsActive.Should().Be(customer.IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentCustomer_ShouldReturnNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _useCase.ExecuteAsync(customerId);

        // Assert
        result.Should().BeNull();
    }
}
