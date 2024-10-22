using MassTransit;
using Shared.Orchestration;
using ShipService.Infrastructure.Enums;
using ShipService.Infrastructure.Context;
using ShipService.Infrastructure.Entities;

namespace ShipService.Consumers
{
    public class PaymentCompletedRequestShippingCreatedConsumer : IConsumer<IOrchestrationShippingRequestedEvent>
    {
        private readonly ILogger<PaymentCompletedRequestShippingCreatedConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly AppDbContext _context;

        public PaymentCompletedRequestShippingCreatedConsumer(AppDbContext context,
          ILogger<PaymentCompletedRequestShippingCreatedConsumer> logger,
          IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _context = context;
        }

        public async Task Consume(ConsumeContext<IOrchestrationShippingRequestedEvent> context)
        {
            var orderId = context.Message.OrderId;
            var shipId = await CreateShipEntryAsync(orderId);
            if (shipId != null)
            {
                Console.WriteLine($"Shipping created for Order ID: {context.Message.OrderId}");
                if (orderId % 2 == 0)
                {
                  //  await _publishEndpoint.Publish(new OrchestrationShippingCompletedEvent(context.Message.CorrelationId));
                }
                else
                {
                 //   await _publishEndpoint.Publish(new OrchestrationPaymentFailedEvent(context.Message.CorrelationId) { Reason = "Shipping faild"});
                }
            }
        }
        private async Task<Ship> CreateShipEntryAsync(int orderId)
        {
            var ship = new Ship
            {
                OrderId = orderId,
                Status = ShipStatus.Suspend,
            };

            _context.Ships.Add(ship);
            await _context.SaveChangesAsync();
            return ship;
        }
    }
}
