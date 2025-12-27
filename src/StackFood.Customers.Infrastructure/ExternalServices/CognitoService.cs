using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using StackFood.Customers.Application.Interfaces;

namespace StackFood.Customers.Infrastructure.ExternalServices;

public class CognitoService : ICognitoService
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient;
    private readonly string _userPoolId;
    private readonly string _clientId;
    private readonly string _defaultPassword;
    private readonly string _guestUsername;
    private readonly string _guestPassword;

    public CognitoService(
        IAmazonCognitoIdentityProvider cognitoClient,
        IConfiguration configuration)
    {
        _cognitoClient = cognitoClient;
        _userPoolId = configuration["Cognito:UserPoolId"] ?? throw new ArgumentNullException("Cognito:UserPoolId");
        _clientId = configuration["Cognito:ClientId"] ?? throw new ArgumentNullException("Cognito:ClientId");
        _defaultPassword = configuration["Cognito:DefaultPassword"] ?? "Stackfood#123";
        _guestUsername = configuration["Cognito:GuestUsername"] ?? "convidado";
        _guestPassword = configuration["Cognito:GuestPassword"] ?? "Convidado123!";
    }

    public async Task<string> CreateUserAsync(string cpf, string email, string name)
    {
        try
        {
            var request = new AdminCreateUserRequest
            {
                UserPoolId = _userPoolId,
                Username = cpf,
                UserAttributes = new List<AttributeType>
                {
                    new AttributeType { Name = "email", Value = email },
                    new AttributeType { Name = "name", Value = name },
                    new AttributeType { Name = "email_verified", Value = "true" }
                },
                TemporaryPassword = _defaultPassword,
                MessageAction = MessageActionType.SUPPRESS
            };

            await _cognitoClient.AdminCreateUserAsync(request);

            // Definir senha permanente
            await _cognitoClient.AdminSetUserPasswordAsync(new AdminSetUserPasswordRequest
            {
                UserPoolId = _userPoolId,
                Username = cpf,
                Password = _defaultPassword,
                Permanent = true
            });

            return cpf;
        }
        catch (UsernameExistsException)
        {
            throw new InvalidOperationException($"User with CPF {cpf} already exists in Cognito");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create user in Cognito: {ex.Message}", ex);
        }
    }

    public async Task<string> AuthenticateAsync(string cpf)
    {
        try
        {
            var request = new InitiateAuthRequest
            {
                ClientId = _clientId,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", cpf },
                    { "PASSWORD", _defaultPassword }
                }
            };

            var response = await _cognitoClient.InitiateAuthAsync(request);
            return response.AuthenticationResult.IdToken;
        }
        catch (NotAuthorizedException)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }
        catch (UserNotFoundException)
        {
            throw new UnauthorizedAccessException("User not found in Cognito");
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException($"Authentication failed: {ex.Message}", ex);
        }
    }

    public async Task<string> AuthenticateGuestAsync()
    {
        try
        {
            var request = new InitiateAuthRequest
            {
                ClientId = _clientId,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", _guestUsername },
                    { "PASSWORD", _guestPassword }
                }
            };

            var response = await _cognitoClient.InitiateAuthAsync(request);
            return response.AuthenticationResult.IdToken;
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException($"Guest authentication failed: {ex.Message}", ex);
        }
    }

    public async Task DeleteUserAsync(string cpf)
    {
        try
        {
            await _cognitoClient.AdminDeleteUserAsync(new AdminDeleteUserRequest
            {
                UserPoolId = _userPoolId,
                Username = cpf
            });
        }
        catch (UserNotFoundException)
        {
            // User already deleted or doesn't exist
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete user from Cognito: {ex.Message}", ex);
        }
    }
}
