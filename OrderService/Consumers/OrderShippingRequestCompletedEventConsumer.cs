using MassTransit;
using OrderService.Infrastructure.Context;
using OrderService.Infrastructure.Enums;
using Shared.Orchestration;

namespace OrderService.Consumers
{
    public class OrderShippingRequestCompletedEventConsumer : IConsumer<IOrchestrationOrderShippingRequestCompletedEvent>
    {

        private readonly AppDbContext _context;
        private readonly ILogger<OrderShippingRequestCompletedEventConsumer> _logger;

        public OrderShippingRequestCompletedEventConsumer(AppDbContext context, ILogger<OrderShippingRequestCompletedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<IOrchestrationOrderShippingRequestCompletedEvent> context)
        {
            var order = await _context.Orders.FindAsync(context.Message.OrderId);
            if (order is not null)
            {
                order.Status = OrderStatus.ShippingCompleted;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order (Id={context.Message.OrderId}) status changed : {order.Status}");
            }
            else
            {
                _logger.LogError($"Order (Id={context.Message.OrderId}) not found");
            }
        }
    }
}
