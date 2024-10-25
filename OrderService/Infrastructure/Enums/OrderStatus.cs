namespace OrderService.Infrastructure.Enums
{
    public enum OrderStatus
    {
        Suspend,
        Completed,       
        Failed,
        ShippingCompleted, 
        ShippingFailed,
        Canceled
    }
}
