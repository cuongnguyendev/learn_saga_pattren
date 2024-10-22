using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Orchestration
{
    public interface IOrchestrationShippingCompletedEvent : CorrelatedBy<Guid>
    {
      public int OrderId {  get; set; } 
    }
    public class OrchestrationShippingCompletedEvent : IOrchestrationShippingCompletedEvent
    {
        public OrchestrationShippingCompletedEvent(int orderId)
        {
           // CorrelationId = correlationId;
            OrderId = orderId;
        }
        public int OrderId { get; set; }
        public Guid CorrelationId { get; }
    }
}
