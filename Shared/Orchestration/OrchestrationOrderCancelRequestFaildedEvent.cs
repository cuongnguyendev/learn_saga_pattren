using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Orchestration
{
    public interface IOrchestrationOrderCancelRequestFaildedEvent
    {
        public int OrderId { get; set; }
        public string Reason { get; set; }
    }
    public class OrchestrationOrderCancelRequestFaildedEvent: IOrchestrationOrderCancelRequestFaildedEvent
    {
        public OrchestrationOrderCancelRequestFaildedEvent(int orderId, string reason)
        {
            OrderId = orderId;
            Reason = reason;
        }
        public int OrderId { get; set; }
        public string Reason { get; set; }
    }
}
