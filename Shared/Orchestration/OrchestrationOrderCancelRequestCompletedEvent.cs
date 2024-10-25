using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Orchestration
{
    public interface IOrchestrationOrderCancelRequestCompletedEvent
    {
        public int OrderId { get; set; }
    }
    public class OrchestrationOrderCancelRequestCompletedEvent : IOrchestrationOrderCancelRequestCompletedEvent
    { 
        public OrchestrationOrderCancelRequestCompletedEvent(int orderId)
        {
            OrderId = orderId;
        }

        public int OrderId { get; set; }
    }
}
