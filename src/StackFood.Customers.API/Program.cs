using Amazon.CognitoIdentityProvider;
using Amazon.SimpleNotificationService;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackFood.Customers.Application.Interfaces;
using StackFood.Customers.Application.UseCases;
using StackFood.Customers.Infrastructure.ExternalServices;
using StackFood.Customers.Infrastructure.Persistence;
using StackFood.Customers.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the containers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "StackFood Customers API", Version = "v1" });
    c.AddServer(new OpenApiServer
    {
        Url = "https://api.stackfood.com.br/customers",
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Database
builder.Services.AddDbContext<CustomersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// AWS Services
builder.Services.AddAWSService<IAmazonCognitoIdentityProvider>();
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();

// Repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// External Services
builder.Services.AddScoped<ICognitoService, CognitoService>();
builder.Services.AddScoped<IEventPublisher, SnsEventPublisher>();

// Use Cases
builder.Services.AddScoped<CreateCustomerUseCase>();
builder.Services.AddScoped<GetCustomerByIdUseCase>();
builder.Services.AddScoped<GetCustomerByCpfUseCase>();
builder.Services.AddScoped<UpdateCustomerUseCase>();
builder.Services.AddScoped<AuthenticateCustomerUseCase>();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CustomersDbContext>();

var app = builder.Build();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();
    dbContext.Database.Migrate();
}

app.UsePathBase("/customers");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/customers/swagger/v1/swagger.json", "StackFood customers API v1");
});

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
