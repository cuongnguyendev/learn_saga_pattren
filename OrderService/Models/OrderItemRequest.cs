﻿namespace OrderService.Models
{
    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
