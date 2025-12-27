using FluentAssertions;
using Moq;
using StackFood.Customers.Application.Interfaces;
using StackFood.Customers.Application.UseCases;
using StackFood.Customers.Domain.Entities;

namespace StackFood.Customers.Tests.UseCases;

public class GetCustomerByCpfUseCaseTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock;
    private readonly GetCustomerByCpfUseCase _useCase;

    public GetCustomerByCpfUseCaseTests()
    {
        _repositoryMock = new Mock<ICustomerRepository>();
        _useCase = new GetCustomerByCpfUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingCustomer_ShouldReturnCustomerDTO()
    {
        // Arrange
        var cpf = "12345678900";
        var customer = new Customer("John Doe", "john@example.com", cpf, "cognito-123");
        _repositoryMock.Setup(r => r.GetByCpfAsync(cpf))
            .ReturnsAsync(customer);

        // Act
        var result = await _useCase.ExecuteAsync(cpf);

        // Assert
        result.Should().NotBeNull();
        result!.Cpf.Should().Be(cpf);
        result.Name.Should().Be(customer.Name);
        result.Email.Should().Be(customer.Email);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentCustomer_ShouldReturnNull()
    {
        // Arrange
        var cpf = "12345678900";
        _repositoryMock.Setup(r => r.GetByCpfAsync(cpf))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _useCase.ExecuteAsync(cpf);

        // Assert
        result.Should().BeNull();
    }
}
