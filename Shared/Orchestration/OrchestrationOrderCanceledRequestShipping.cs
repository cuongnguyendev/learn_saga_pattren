using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Orchestration
{
    public interface IOrchestrationOrderCanceledRequestShipping
    {
       public int OrderId {  get; set; }
    }
    public class OrchestrationOrderCanceledRequestShipping: IOrchestrationOrderCanceledRequestShipping
    {
        public OrchestrationOrderCanceledRequestShipping(int orderId) 
        {
            OrderId = orderId;
        }
        public int OrderId { get; set; }
    }
}
