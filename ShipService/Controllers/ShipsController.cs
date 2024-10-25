using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using Shared.Orchestration;
using ShipService.Infrastructure.Context;
using ShipService.Infrastructure.Entities;
using ShipService.Infrastructure.Enums;

namespace ShipService.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ShipsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        public ShipsController(AppDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<IActionResult> All(CancellationToken cancellationToken)
        {
            var ships = await _context.Ships.ToListAsync(cancellationToken);
            if (!ships.Any())
            {
                return NotFound();
            }

            return Ok(ships);
        }

        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] ShipStatus newStatus, CancellationToken cancellationToken)
        {
            var shipUpdated = await UpdateStatusAsync(orderId, newStatus, cancellationToken);

            if (shipUpdated == null)
            {
                return NotFound();
            }
            if (newStatus == ShipStatus.Completed)
            {
                await _publishEndpoint.Publish(new OrchestrationShippingCompletedEvent(shipUpdated.OrderId));
            }
            else
            {
                await _publishEndpoint.Publish(new OrchestrationShippingFailedEvent(shipUpdated.OrderId) { Reason = "Shipping faild" });
               
            }
            return NoContent();
        }

        private async Task<Ship> UpdateStatusAsync(int orderId, ShipStatus newStatus, CancellationToken cancellationToken)
        {
            var ship = await _context.Ships.Where(s=>s.OrderId==orderId).FirstOrDefaultAsync();
            if (ship == null || ship.Status!=ShipStatus.Suspend)
            {
                return null;
            }

            ship.Status = newStatus;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return ship;
            }
            catch (DbUpdateException ex)
            {
                // Log the exception (consider using a logging framework)
                return null;
            }
        }

       
    }
}
