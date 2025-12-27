using FluentAssertions;
using Moq;
using StackFood.Customers.Application.DTOs;
using StackFood.Customers.Application.Interfaces;
using StackFood.Customers.Application.UseCases;
using StackFood.Customers.Domain.Entities;

namespace StackFood.Customers.Tests.UseCases;

public class AuthenticateCustomerUseCaseTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock;
    private readonly Mock<ICognitoService> _cognitoServiceMock;
    private readonly AuthenticateCustomerUseCase _useCase;

    public AuthenticateCustomerUseCaseTests()
    {
        _repositoryMock = new Mock<ICustomerRepository>();
        _cognitoServiceMock = new Mock<ICognitoService>();
        _useCase = new AuthenticateCustomerUseCase(_repositoryMock.Object, _cognitoServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyCpf_ShouldAuthenticateAsGuest()
    {
        // Arrange
        var request = new AuthenticateRequest { Cpf = "" };
        _cognitoServiceMock.Setup(c => c.AuthenticateGuestAsync())
            .ReturnsAsync("guest-token-123");

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("guest-token-123");
        result.Customer.Should().BeNull();
        result.Message.Should().Contain("guest");

        _cognitoServiceMock.Verify(c => c.AuthenticateGuestAsync(), Times.Once);
        _repositoryMock.Verify(r => r.GetByCpfAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullCpf_ShouldAuthenticateAsGuest()
    {
        // Arrange
        var request = new AuthenticateRequest { Cpf = null };
        _cognitoServiceMock.Setup(c => c.AuthenticateGuestAsync())
            .ReturnsAsync("guest-token-123");

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Token.Should().Be("guest-token-123");
        result.Message.Should().Contain("guest");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCpf_ShouldAuthenticateCustomer()
    {
        // Arrange
        var request = new AuthenticateRequest { Cpf = "12345678900" };
        var customer = new Customer("John Doe", "john@example.com", request.Cpf, "cognito-123");

        _repositoryMock.Setup(r => r.GetByCpfAsync(request.Cpf))
            .ReturnsAsync(customer);
        _cognitoServiceMock.Setup(c => c.AuthenticateAsync(request.Cpf))
            .ReturnsAsync("customer-token-123");

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("customer-token-123");
        result.Customer.Should().NotBeNull();
        result.Customer.Cpf.Should().Be(request.Cpf);
        result.Message.Should().Contain("successfully");

        _cognitoServiceMock.Verify(c => c.AuthenticateAsync(request.Cpf), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentCustomer_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var request = new AuthenticateRequest { Cpf = "12345678900" };
        _repositoryMock.Setup(r => r.GetByCpfAsync(request.Cpf))
            .ReturnsAsync((Customer?)null);

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Customer not found*");
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveCustomer_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var request = new AuthenticateRequest { Cpf = "12345678900" };
        var customer = new Customer("John Doe", "john@example.com", request.Cpf);
        customer.Deactivate();

        _repositoryMock.Setup(r => r.GetByCpfAsync(request.Cpf))
            .ReturnsAsync(customer);

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*inactive*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCognitoFails_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var request = new AuthenticateRequest { Cpf = "12345678900" };
        var customer = new Customer("John Doe", "john@example.com", request.Cpf);

        _repositoryMock.Setup(r => r.GetByCpfAsync(request.Cpf))
            .ReturnsAsync(customer);
        _cognitoServiceMock.Setup(c => c.AuthenticateAsync(request.Cpf))
            .ThrowsAsync(new Exception("Cognito error"));

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Authentication failed*");
    }
}
