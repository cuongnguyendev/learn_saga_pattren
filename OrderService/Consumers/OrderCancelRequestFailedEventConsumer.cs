using MassTransit;
using OrderService.Infrastructure.Context;
using OrderService.Infrastructure.Enums;
using Shared.Orchestration;

namespace OrderService.Consumers
{
    public class OrderCancelRequestFailedEventConsumer:IConsumer<IOrchestrationOrderCancelRequestFaildedEvent>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderCancelRequestFailedEventConsumer> _logger;

        public OrderCancelRequestFailedEventConsumer(AppDbContext context, ILogger<OrderCancelRequestFailedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IOrchestrationOrderCancelRequestFaildedEvent> context)
        {
            var order = await _context.Orders.FindAsync(context.Message.OrderId);

            if (order != null)
            {
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
