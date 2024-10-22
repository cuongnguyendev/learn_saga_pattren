using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Orchestration
{
    public interface IOrchestrationShippingRequestedEvent: CorrelatedBy<Guid>
    {
        public int OrderId { get; set; }
    }

    public class OrchestrationShippingRequestedEvent : IOrchestrationShippingRequestedEvent
    {
        public int OrderId { get;set; }
        public OrchestrationShippingRequestedEvent( int orderId,Guid correlationId)
        {
           
            OrderId = orderId;
            CorrelationId = correlationId;  
        }
        public Guid CorrelationId { get; }
    }
}
