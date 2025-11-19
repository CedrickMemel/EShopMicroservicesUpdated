namespace Ordering.Application.Orders.EventHandlers;

internal class OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger) : INotificationHandler<OrderCreatedEvent>
{
    public Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("OrderCreatedEventHandler called for OrderId: {OrderId}", notification.order.Id);
        return Task.CompletedTask;
    }
}
