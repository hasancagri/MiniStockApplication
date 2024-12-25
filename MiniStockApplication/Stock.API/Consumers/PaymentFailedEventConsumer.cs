using MassTransit;

using Shared;

namespace Stock.API.Consumers;

public class PaymentFailedEventConsumer
    : IConsumer<PaymentFailedEvent>
{
    private ILogger<PaymentFailedEventConsumer> _logger;

    public PaymentFailedEventConsumer(ILogger<PaymentFailedEventConsumer> logger)
        => _logger = logger;

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        _logger.LogInformation($"Stock was released for Order Id ({context.Message.orderId})");
    }
}