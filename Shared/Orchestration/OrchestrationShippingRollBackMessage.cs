using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Orchestration
{
    public interface IOrchestrationShippingRollBackMessage
    {
        List<OrderItemMessage> OrderItems { get; set; }
    }
    public class OrchestrationShippingRollBackMessage:IOrchestrationStockRollBackMessage
    {
        public OrchestrationShippingRollBackMessage(List<OrderItemMessage> orderItems)
        {
            OrderItems = orderItems;
        }

        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
