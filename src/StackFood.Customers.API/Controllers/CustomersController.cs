using Microsoft.AspNetCore.Mvc;
using StackFood.Customers.Application.DTOs;
using StackFood.Customers.Application.UseCases;

namespace StackFood.Customers.API.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly CreateCustomerUseCase _createCustomerUseCase;
    private readonly GetCustomerByIdUseCase _getCustomerByIdUseCase;
    private readonly GetCustomerByCpfUseCase _getCustomerByCpfUseCase;
    private readonly UpdateCustomerUseCase _updateCustomerUseCase;
    private readonly AuthenticateCustomerUseCase _authenticateCustomerUseCase;

    public CustomersController(
        CreateCustomerUseCase createCustomerUseCase,
        GetCustomerByIdUseCase getCustomerByIdUseCase,
        GetCustomerByCpfUseCase getCustomerByCpfUseCase,
        UpdateCustomerUseCase updateCustomerUseCase,
        AuthenticateCustomerUseCase authenticateCustomerUseCase)
    {
        _createCustomerUseCase = createCustomerUseCase;
        _getCustomerByIdUseCase = getCustomerByIdUseCase;
        _getCustomerByCpfUseCase = getCustomerByCpfUseCase;
        _updateCustomerUseCase = updateCustomerUseCase;
        _authenticateCustomerUseCase = authenticateCustomerUseCase;
    }

    /// <summary>
    /// Creates a new customer in the database and Cognito.
    /// </summary>
    /// <param name="request">Customer data (name, email, cpf)</param>
    /// <returns>Created customer</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        try
        {
            var customer = await _createCustomerUseCase.ExecuteAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get customer by ID.
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer data</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _getCustomerByIdUseCase.ExecuteAsync(id);
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    /// <summary>
    /// Get customer by CPF.
    /// </summary>
    /// <param name="cpf">Customer CPF</param>
    /// <returns>Customer data</returns>
    [HttpGet("cpf/{cpf}")]
    [ProducesResponseType(typeof(CustomerDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCpf(string cpf)
    {
        var customer = await _getCustomerByCpfUseCase.ExecuteAsync(cpf);
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    /// <summary>
    /// Update customer information.
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="request">Updated customer data</param>
    /// <returns>Updated customer</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        try
        {
            var customer = await _updateCustomerUseCase.ExecuteAsync(id, request);
            return Ok(customer);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Authenticate customer via CPF.
    /// </summary>
    /// <param name="request">Authentication request with CPF</param>
    /// <returns>JWT Token and customer data</returns>
    [HttpPost("auth")]
    [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
    {
        try
        {
            var response = await _authenticateCustomerUseCase.ExecuteAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
