using MassTransit;
using OrderService.Infrastructure.Context;
using OrderService.Infrastructure.Enums;
using Shared.Orchestration;

namespace OrderService.Consumers
{
    public class OrderShippingRequestFailedEventConsumer : IConsumer<IOrchestrationOrderShippingRequestFailedEvent>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderShippingRequestFailedEventConsumer> _logger;

        public OrderShippingRequestFailedEventConsumer(AppDbContext context, ILogger<OrderShippingRequestFailedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<IOrchestrationOrderShippingRequestFailedEvent> context)
        {
            var order = await _context.Orders.FindAsync(context.Message.OrderId);

            if (order != null)
            {
                order.Status = OrderStatus.ShippingFailed;
                order.FailMessage = context.Message.Reason;
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
