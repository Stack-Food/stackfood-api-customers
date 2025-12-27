namespace StackFood.Customers.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T eventMessage, string topicArn) where T : class;
}
