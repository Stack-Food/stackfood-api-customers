using FluentAssertions;
using Moq;
using StackFood.Customers.Application.DTOs;
using StackFood.Customers.Application.Interfaces;
using StackFood.Customers.Application.UseCases;
using StackFood.Customers.Domain.Entities;

namespace StackFood.Customers.Tests.UseCases;

public class CreateCustomerUseCaseTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock;
    private readonly Mock<ICognitoService> _cognitoServiceMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly CreateCustomerUseCase _useCase;

    public CreateCustomerUseCaseTests()
    {
        _repositoryMock = new Mock<ICustomerRepository>();
        _cognitoServiceMock = new Mock<ICognitoService>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _useCase = new CreateCustomerUseCase(
            _repositoryMock.Object,
            _cognitoServiceMock.Object,
            _eventPublisherMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Cpf = "12345678900"
        };

        _repositoryMock.Setup(r => r.GetByCpfAsync(request.Cpf))
            .ReturnsAsync((Customer?)null);
        _repositoryMock.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((Customer?)null);
        _cognitoServiceMock.Setup(c => c.CreateUserAsync(request.Cpf, request.Email, request.Name))
            .ReturnsAsync("cognito-user-123");
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync((Customer c) => c);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Email.Should().Be(request.Email);
        result.Cpf.Should().Be(request.Cpf);
        result.CognitoUserId.Should().Be("cognito-user-123");
        result.IsActive.Should().BeTrue();

        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Once);
        _cognitoServiceMock.Verify(c => c.CreateUserAsync(request.Cpf, request.Email, request.Name), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateCpf_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Cpf = "12345678900"
        };

        var existingCustomer = new Customer("Jane Doe", "jane@example.com", request.Cpf);
        _repositoryMock.Setup(r => r.GetByCpfAsync(request.Cpf))
            .ReturnsAsync(existingCustomer);

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CPF already exists*");

        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Never);
        _cognitoServiceMock.Verify(c => c.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Cpf = "12345678900"
        };

        var existingCustomer = new Customer("Jane Doe", request.Email, "98765432100");
        _repositoryMock.Setup(r => r.GetByCpfAsync(request.Cpf))
            .ReturnsAsync((Customer?)null);
        _repositoryMock.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingCustomer);

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email already exists*");

        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Never);
        _cognitoServiceMock.Verify(c => c.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCognitoFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Cpf = "12345678900"
        };

        _repositoryMock.Setup(r => r.GetByCpfAsync(request.Cpf))
            .ReturnsAsync((Customer?)null);
        _repositoryMock.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((Customer?)null);
        _cognitoServiceMock.Setup(c => c.CreateUserAsync(request.Cpf, request.Email, request.Name))
            .ThrowsAsync(new Exception("Cognito error"));

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to create user in Cognito*");

        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Never);
    }
}
