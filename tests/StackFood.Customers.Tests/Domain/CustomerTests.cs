using FluentAssertions;
using StackFood.Customers.Domain.Entities;

namespace StackFood.Customers.Tests.Domain;

public class CustomerTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateCustomer()
    {
        // Arrange & Act
        var customer = new Customer("John Doe", "john@example.com", "12345678900");

        // Assert
        customer.Id.Should().NotBeEmpty();
        customer.Name.Should().Be("John Doe");
        customer.Email.Should().Be("john@example.com");
        customer.Cpf.Should().Be("12345678900");
        customer.CognitoUserId.Should().BeNull();
        customer.IsActive.Should().BeTrue();
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithCognitoUserId_ShouldSetCognitoUserId()
    {
        // Arrange & Act
        var cognitoUserId = "cognito-123";
        var customer = new Customer("John Doe", "john@example.com", "12345678900", cognitoUserId);

        // Assert
        customer.CognitoUserId.Should().Be(cognitoUserId);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new Customer("", "john@example.com", "12345678900");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name")
            .WithMessage("*Name cannot be empty*");
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new Customer(null!, "john@example.com", "12345678900");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new Customer("   ", "john@example.com", "12345678900");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Constructor_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new Customer("John Doe", "", "12345678900");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("email")
            .WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmptyCpf_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new Customer("John Doe", "john@example.com", "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("cpf")
            .WithMessage("*CPF cannot be empty*");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateCustomer()
    {
        // Arrange
        var customer = new Customer("John Doe", "john@example.com", "12345678900");
        var originalUpdatedAt = customer.UpdatedAt;

        // Act
        customer.Update("Jane Doe", "jane@example.com");

        // Assert
        customer.Name.Should().Be("Jane Doe");
        customer.Email.Should().Be("jane@example.com");
        customer.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var customer = new Customer("John Doe", "john@example.com", "12345678900");

        // Act
        var act = () => customer.Update("", "jane@example.com");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Update_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var customer = new Customer("John Doe", "john@example.com", "12345678900");

        // Act
        var act = () => customer.Update("Jane Doe", "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("email");
    }

    [Fact]
    public void SetCognitoUserId_WithValidId_ShouldSetCognitoUserId()
    {
        // Arrange
        var customer = new Customer("John Doe", "john@example.com", "12345678900");

        // Act
        customer.SetCognitoUserId("cognito-123");

        // Assert
        customer.CognitoUserId.Should().Be("cognito-123");
    }

    [Fact]
    public void SetCognitoUserId_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange
        var customer = new Customer("John Doe", "john@example.com", "12345678900");

        // Act
        var act = () => customer.SetCognitoUserId("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("cognitoUserId");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var customer = new Customer("John Doe", "john@example.com", "12345678900");

        // Act
        customer.Deactivate();

        // Assert
        customer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var customer = new Customer("John Doe", "john@example.com", "12345678900");
        customer.Deactivate();

        // Act
        customer.Activate();

        // Assert
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldUpdateTimestamp()
    {
        // Arrange
        var customer = new Customer("John Doe", "john@example.com", "12345678900");
        var originalUpdatedAt = customer.UpdatedAt;

        // Act
        customer.Deactivate();

        // Assert
        customer.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Activate_ShouldUpdateTimestamp()
    {
        // Arrange
        var customer = new Customer("John Doe", "john@example.com", "12345678900");
        customer.Deactivate();
        var originalUpdatedAt = customer.UpdatedAt;

        // Act
        customer.Activate();

        // Assert
        customer.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}
