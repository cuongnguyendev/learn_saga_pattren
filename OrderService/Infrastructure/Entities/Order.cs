using OrderService.Infrastructure.Enums;
using System.Net;

namespace OrderService.Infrastructure.Entities
{
    public class Order
    {
        public Order()
        {
            Items = new List<OrderItem>();
        }

        public int Id { get; set; }
        public string BuyerId { get; set; }
        public DateTime CreatedDate { get; set; }
        public OrderStatus Status { get; set; }
        public string FailMessage { get; set; }
        public Address Address { get; set; }

        public List<OrderItem> Items { get; set; }
    }
}
