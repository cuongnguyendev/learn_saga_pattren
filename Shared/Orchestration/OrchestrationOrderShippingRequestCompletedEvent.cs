using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Orchestration
{
    public interface IOrchestrationOrderShippingRequestCompletedEvent
    {
        public int OrderId { get; set; }
    }
    public class OrchestrationOrderShippingRequestCompletedEvent: IOrchestrationOrderShippingRequestCompletedEvent
    {
        public OrchestrationOrderShippingRequestCompletedEvent(int orderId)
        {
            OrderId = orderId;
        }

        public int OrderId { get; set; }
    }
}
