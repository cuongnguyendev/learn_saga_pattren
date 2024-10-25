using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Orchestration;
using ShipService.Infrastructure.Context;
using ShipService.Infrastructure.Enums;

namespace ShipService.Consumers
{
    public class OrderCanceledRequestShippingConsumer: IConsumer<OrchestrationOrderCanceledRequestShipping>
    {
        private readonly ILogger<PaymentCompletedRequestShippingCreatedConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly AppDbContext _context;

        public OrderCanceledRequestShippingConsumer(AppDbContext context,
          ILogger<PaymentCompletedRequestShippingCreatedConsumer> logger,
          IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _context = context;
        }

        public async Task Consume(ConsumeContext<OrchestrationOrderCanceledRequestShipping> context)
        {
            var ship = await _context.Ships.Where(s=>s.OrderId==context.Message.OrderId).FirstOrDefaultAsync();

            if (ship != null)
            {
                ship.Status = ShipStatus.Canceled;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order (Id={context.Message.OrderId}) status changed : {ship.Status}");
            }
            else
            {
                _logger.LogError($"ship (Id={context.Message.OrderId}) not found");
            }
        }
    }
}

 