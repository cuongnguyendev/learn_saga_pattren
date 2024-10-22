using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Orchestration
{
    public interface IOrchestrationShippingFailedEvent:CorrelatedBy<Guid>
    {
        public int OrderId { get; set; }
        public string Reason { get; set; }
    }
    public class OrchestrationShippingFailedEvent : IOrchestrationShippingFailedEvent
    {
        public OrchestrationShippingFailedEvent(int orderId)
        {
            OrderId = orderId;
        }
        public string Reason { get; set; }
        public int OrderId { get; set; }
        public Guid CorrelationId { get; }
    }
}
