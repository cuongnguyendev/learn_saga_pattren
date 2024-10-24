using StateMachineWorkerService.Models;

namespace StateMachineWorkerService.CustomSate
{
    public class OrderStateInstance : SagaStateMachineInstance
    {
        //A random correlation ID is generated for each line.
        //It is used to distinguish the states of the state machine from one another.
        public Guid CorrelationId { get; set; } //Comes via the SagaStateMachineInstance interface

        public string CurrentState { get; set; }
        public string BuyerId { get; set; }
        public int OrderId { get; set; }
        public string OrderItems {  get; set; }
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVV { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedDate { get; set; }

        public override string ToString()
        {
            var properties = GetType().GetProperties().ToList();

            StringBuilder stringBuilder = new();

            properties.ForEach(p =>
            {
                var value = p.GetValue(this, null);
                stringBuilder.AppendLine($"{p.Name}:{value}");
            });

            stringBuilder.Append("------------------------");
            return stringBuilder.ToString();
        }
    }
}
