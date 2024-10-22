using ShipService.Infrastructure.Enums;

namespace ShipService.Infrastructure.Entities
{
    public class Ship
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public ShipStatus Status { get; set; }
    }
}
