using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using StackFood.Customers.Application.Interfaces;
using System.Text.Json;

namespace StackFood.Customers.Infrastructure.ExternalServices;

public class SnsEventPublisher : IEventPublisher
{
    private readonly IAmazonSimpleNotificationService _snsClient;

    public SnsEventPublisher(IAmazonSimpleNotificationService snsClient)
    {
        _snsClient = snsClient;
    }

    public async Task PublishAsync<T>(T eventMessage, string topicArn) where T : class
    {
        try
        {
            var messageJson = JsonSerializer.Serialize(eventMessage, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var request = new PublishRequest
            {
                TopicArn = topicArn,
                Message = messageJson,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "eventType",
                        new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = typeof(T).Name
                        }
                    }
                }
            };

            await _snsClient.PublishAsync(request);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - events are fire and forget
            Console.WriteLine($"Failed to publish event: {ex.Message}");
        }
    }
}
