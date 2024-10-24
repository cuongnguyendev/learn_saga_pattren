using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Orchestration
{
    public interface IOrchestrationOrderCancellationEvent : CorrelatedBy<Guid>
    {
        public int OrderId { get; set; }
        public string Reason { get; set; }
    }
    public class OrchestrationOrderCancellationEvent: IOrchestrationOrderCancellationEvent
    {
        public int OrderId { get; set; }
        public string Reason { get; set; }
        public Guid CorrelationId { get;} 
        
    }
}
