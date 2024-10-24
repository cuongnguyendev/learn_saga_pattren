using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OrderService.Infrastructure.Context;
using OrderService.Infrastructure.Entities;
using OrderService.Infrastructure.Enums;
using OrderService.Models;
using Shared.Infrastructure;
using Shared.Models;
using Shared.Orchestration;

namespace OrderService.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public OrdersController(
            AppDbContext context,
            ISendEndpointProvider sendEndpointProvider)
        {
            _context = context;
            _sendEndpointProvider = sendEndpointProvider;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateRequest request)
        {
            var order = await AddOrderAsync(request);

            OrderCreatedRequestEvent orderCreatedRequestEvent = new()
            {
                BuyerId = request.BuyerId,
                OrderId = order.Id,
                Payment = new PaymentMessage
                {
                    CardName = request.Payment.CardName,
                    CardNumber = request.Payment.CardNumber,
                    Expiration = request.Payment.Expiration,
                    CVV = request.Payment.CVV,
                    TotalPrice = request.OrderItems.Sum(x => x.Price * x.Count)
                },
            };

            request.OrderItems.ForEach(item =>
            {
                orderCreatedRequestEvent.OrderItems.Add(new OrderItemMessage
                {
                    Count = item.Count,
                    ProductId = item.ProductId
                });
            });

            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitQueueName.OrderSaga}"));
            await sendEndpoint.Send<IOrderCreatedRequestEvent>(orderCreatedRequestEvent);

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> CancelOrder(CancelOrderRequest request)
        {
            var order = await _context.Orders.FindAsync(request.OrderId);

            if (order == null)
            {
                return NotFound($"Order with ID {request.OrderId} not found.");
            }

            if (order.Status == OrderStatus.Shipped) // Assuming 'Shipped' is the state after shipping is completed
            {
                return BadRequest("Order cannot be cancelled after it has been shipped.");
            }

            // Generate a new CorrelationId for the cancellation event
            var correlationId = Guid.NewGuid();

            // Pass the correlationId to the constructor
            var cancellationEvent = new OrchestrationOrderCancellationEvent
            {
                OrderId = order.Id,
                Reason = request.Reason
            };

            // Send cancellation event
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitQueueName.OrderSaga}"));
            await sendEndpoint.Send<IOrchestrationOrderCancellationEvent>(cancellationEvent);

            return Ok("Order cancellation request has been sent successfully.");
        }
        private async Task<Order> AddOrderAsync(OrderCreateRequest request)
        {
            Order order = new()
            {
                BuyerId = request.BuyerId,
                Status = OrderStatus.Suspend,
                FailMessage = string.Empty,
                Address = new Address
                {
                    Line = request.Address.Line,
                    Province = request.Address.Province,
                    District = request.Address.District
                },
                CreatedDate = DateTime.Now
            };

            request.OrderItems.ForEach(requestOrderItem =>
            {
                order.Items.Add(new OrderItem()
                {
                    Price = requestOrderItem.Price,
                    ProductId = requestOrderItem.ProductId,
                    Count = requestOrderItem.Count
                });
            });

            await _context.AddAsync(order);
            await _context.SaveChangesAsync();

            return order;
        }
    }

}
