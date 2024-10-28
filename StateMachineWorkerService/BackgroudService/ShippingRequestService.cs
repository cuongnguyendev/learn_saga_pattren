using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineWorkerService.BackgroudService
{
    public class ShippingRequestService
    {
        private readonly AppDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        public ShippingRequestService(AppDbContext dbContext, IPublishEndpoint publishEndpoint)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
        }
        public async Task CheckAndCallShippingRequest(CancellationToken cancellationToken)
        {
            // Get the time for orders older than 2 hours
            var twoHoursAgo = DateTime.UtcNow.AddMinutes(-2);

            // Query the database for orders older than 2 hours
            var oldOrders = await _dbContext.OrderStateInstances
                .Where(o => o.CreatedDate < twoHoursAgo && o.CurrentState == "PaymentCompleted")
                 .Take(20) // Limit to top 20 records
                .ToListAsync(cancellationToken);
            Console.WriteLine(oldOrders);
            foreach (var order in oldOrders)
            {
                await _publishEndpoint.Publish(new OrchestrationShippingRequestedEvent(order.OrderId, order.CorrelationId));
            }
        }
    }
}
