using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Orchestration
{
    public interface IOrchestrationOrderShippingRequestFailedEvent
    {
        public int OrderId { get; set; }
        public string Reason { get; set; }
    }

    public class OrchestrationOrderShippingRequestFailedEvent: IOrchestrationOrderShippingRequestFailedEvent
    {
        public OrchestrationOrderShippingRequestFailedEvent(int orderId, string reason)
        {
            OrderId = orderId;
            Reason = reason;
        }

        public int OrderId { get; set; }
        public string Reason { get; set; }
    }
}
