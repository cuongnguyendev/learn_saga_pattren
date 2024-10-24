using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineWorkerService.Models
{
    public class ItemOrders
    {
        public int Id { get; set; } 
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Count { get; set; }
        public Guid OrderStateInstanceId { get; set; }
        public OrderStateInstance OrderStateInstance { get; set; }
    }
}
