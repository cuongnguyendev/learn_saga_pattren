using MassTransit;
using OrderService.Infrastructure.Context;
using OrderService.Infrastructure.Enums;
using Shared.Orchestration;

namespace OrderService.Consumers
{
    public class OrderCancelRequestCompletedEventConsumer: IConsumer<IOrchestrationOrderCancelRequestCompletedEvent>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderCancelRequestCompletedEventConsumer> _logger;

        public OrderCancelRequestCompletedEventConsumer(AppDbContext context, ILogger<OrderCancelRequestCompletedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IOrchestrationOrderCancelRequestCompletedEvent> context)
        {
            var order = await _context.Orders.FindAsync(context.Message.OrderId);

            if (order != null)
            {
                order.Status = OrderStatus.Canceled;
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
