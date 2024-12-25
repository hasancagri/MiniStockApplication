using MassTransit;

using Shared;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer
    : IConsumer<OrderCreatedEvent>
{
    private readonly List<Models.Stock> _stocks =
    [
         new Models.Stock { ProductId = 1, Count = 10 },
         new Models.Stock { ProductId = 2, Count = 20 },
         new Models.Stock { ProductId = 3, Count = 30 },
         new Models.Stock { ProductId = 4, Count = 3 },
    ];

    private readonly ILogger<OrderCreatedEventConsumer> _logger;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderCreatedEventConsumer(ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _sendEndpointProvider = sendEndpointProvider;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var stockResult = new List<bool>();

        foreach (var item in context.Message.OrderItems)
        {
            stockResult.Add(_stocks.Any(x => x.ProductId == item.ProductId && x.Count > item.Count));
        }

        if (stockResult.All(x => x.Equals(true)))
        {
            foreach (var item in context.Message.OrderItems)
            {
                var stock = _stocks.FirstOrDefault(x => x.ProductId == item.ProductId);

                if (stock != null)
                {
                    stock.Count -= item.Count;
                }
            }

            _logger.LogInformation($"Stock was reserved for Buyer Id :{context.Message.BuyerId}");

            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettingsConst.StockReservedEventQueueName}"));

            StockReservedEvent stockReservedEvent = new()
            {
                Payment = context.Message.Payment,
                BuyerId = context.Message.BuyerId,
                OrderId = context.Message.OrderId,
                OrderItems = context.Message.OrderItems
            };

            await sendEndpoint.Send(stockReservedEvent);
        }
        else
        {
            await _publishEndpoint.Publish(new StockNotReservedEvent()
            {
                OrderId = context.Message.OrderId,
                Message = "Not enough stock"
            });

            _logger.LogInformation($"Not enough stock for Buyer Id :{context.Message.BuyerId}");
        }
    }
}
