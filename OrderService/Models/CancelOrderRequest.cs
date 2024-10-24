namespace OrderService.Models
{
    public class CancelOrderRequest
    {
        public int OrderId { get; set; }
        public string Reason { get; set; }
    }
}
