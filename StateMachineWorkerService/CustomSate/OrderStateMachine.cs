// File: OrderStateMachine.cs
namespace StateMachineWorkerService.CustomState
{
    using MassTransit;
    using MassTransit.Internals;
    using Shared.Orchestration;
    using System;

    /// <summary>
    /// This class is where I manage the entire distributed transaction.
    /// Then: The place where you will write the business logic.
    /// </summary>
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        // Events
        public Event<IOrderCreatedRequestEvent> OrderCreatedRequestEvent { get; set; }
        public Event<IOrchestrationStockReservedEvent> StockReservedEvent { get; set; }
        public Event<IOrchestrationStockNotReservedEvent> StockNotReservedEvent { get; set; }
        public Event<IOrchestrationPaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<IOrchestrationPaymentFailedEvent> PaymentFailedEvent { get; set; }
        public Event<IOrchestrationShippingRequestedEvent> ShippingRequestedEvent { get; set; }
        public Event<IOrchestrationShippingCompletedEvent> ShippingCompletedEvent { get; set; }
        public Event<IOrchestrationShippingFailedEvent> ShippingFailedEvent { get; set; }

        // States
        public State OrderCreated { get; set; }
        public State StockReserved { get; set; }
        public State StockNotReserved { get; set; }
        public State PaymentCompleted { get; set; }
        public State PaymentFailed { get; set; }
        public State ShippingRequested { get; set; }
        public State ShippingCompleted { get; set; }
        public State ShippingFailed { get; set; }

        public OrderStateMachine()
        {
            InstanceState(instance => instance.CurrentState);

            Event(() => OrderCreatedRequestEvent, eventCorrelationConfigurator =>
            {
                eventCorrelationConfigurator.CorrelateBy<int>(database => database.OrderId, @event => @event.Message.OrderId)
                    .SelectId(selector => Guid.NewGuid());
            });

            Event(() => StockReservedEvent, eventCorrelationConfigurator =>
            {
                eventCorrelationConfigurator.CorrelateById(context => context.Message.CorrelationId);
            });

            Event(() => StockNotReservedEvent, eventCorrelationConfigurator =>
            {
                eventCorrelationConfigurator.CorrelateById(context => context.Message.CorrelationId);
            });

            Event(() => PaymentCompletedEvent, eventCorrelationConfigurator =>
            {
                eventCorrelationConfigurator.CorrelateById(context => context.Message.CorrelationId);
            });
            Event(() => ShippingRequestedEvent, eventCorrelationConfigurator =>
            {
                eventCorrelationConfigurator.CorrelateById(context => context.Message.CorrelationId);
            });
            //Event(() => ShippingCompletedEvent, eventCorrelationConfigurator =>
            //{
            //    eventCorrelationConfigurator.CorrelateById(context => context.Message.CorrelationId);
            //});

            Event(() => ShippingCompletedEvent, eventCorrelationConfigurator =>
            {
                eventCorrelationConfigurator.CorrelateBy<int>(instance => instance.OrderId, context => context.Message.OrderId);
            });

            //Event(() => ShippingFailedEvent, eventCorrelationConfigurator =>
            //{
            //    eventCorrelationConfigurator.CorrelateById(context => context.Message.CorrelationId);
            //});

            Event(() => ShippingFailedEvent, eventCorrelationConfigurator =>
            {
                eventCorrelationConfigurator.CorrelateBy<int>(instance => instance.OrderId, context => context.Message.OrderId);
            });
            // Handling Order Created Event
            Initially(
                When(OrderCreatedRequestEvent)
                    .Then(context =>
                    {
                        context.Saga.BuyerId = context.Message.BuyerId;
                        context.Saga.OrderId = context.Message.OrderId;
                        context.Saga.CreatedDate = DateTime.Now;
                        context.Saga.CardName = context.Message.Payment.CardName;
                        context.Saga.CardNumber = context.Message.Payment.CardNumber;
                        context.Saga.CVV = context.Message.Payment.CVV;
                        context.Saga.Expiration = context.Message.Payment.Expiration;
                        context.Saga.TotalPrice = context.Message.Payment.TotalPrice;
                    })
                    .Publish(context => new OrchestrationOrderCreatedEvent(context.CorrelationId.Value)
                    {
                        OrderItems = context.Message.OrderItems
                    })
                    .TransitionTo(OrderCreated)
            );

            // While in OrderCreated state
            During(OrderCreated,
                When(StockReservedEvent)
                    .Send(new Uri($"queue:{RabbitQueueName.PaymentStockReservedRequestQueueName}"), context => new OrchestrationStockReservedRequestPayment(context.Message.CorrelationId)
                    {
                        OrderId=context.Saga.OrderId,
                        OrderItems = context.Message.OrderItems,
                        Payment = new PaymentMessage
                        {
                            CardName = context.Saga.CardName,
                            CardNumber = context.Saga.CardNumber,
                            CVV = context.Saga.CVV,
                            Expiration = context.Saga.Expiration,
                            TotalPrice = context.Saga.TotalPrice
                        },
                        BuyerId = context.Saga.BuyerId
                    })
                    .TransitionTo(StockReserved),

                When(StockNotReservedEvent)
                    .Publish(context => new OrchestrationOrderRequestFailedEvent(context.Saga.OrderId, context.Message.Reason))
                    .TransitionTo(StockNotReserved)
            );

            // While in StockReserved state
            During(StockReserved,
                When(PaymentCompletedEvent)
                    .Publish(context => new OrchestrationOrderRequestCompletedEvent(context.Saga.OrderId))
                    .TransitionTo(PaymentCompleted),
   

                When(PaymentFailedEvent)
                    .Publish(context => new OrchestrationOrderRequestFailedEvent(context.Saga.OrderId, context.Message.Reason))
                    .Send(new Uri($"queue:{RabbitQueueName.StockRollBackMessageQueueName}"), context => new OrchestrationStockRollBackMessage(context.Message.OrderItems))
                    .TransitionTo(PaymentFailed)
            );

            // While in PaymentCompleted state, handle shipping requests
            During(PaymentCompleted,
                When(ShippingRequestedEvent)
                    .Send(new Uri($"queue:{RabbitQueueName.ShippingRequestQueueName}"), context => new ShippingRequestMessage
                    {
                        OrderId = context.Saga.OrderId,          
                    })

                    .TransitionTo(ShippingRequested)
            );

            //// While in ShippingRequested state
            During(ShippingRequested,
                When(ShippingCompletedEvent)
                    .Then(context =>
                    {
                        Console.WriteLine($"OrchestrationOrderShippingRequestCompletedEvent After : {context.Saga}");
                    })
                    .Publish(context => new OrchestrationOrderShippingRequestCompletedEvent(context.Saga.OrderId))
                    .TransitionTo(ShippingCompleted)
                   .Finalize(),

                When(ShippingFailedEvent)
                    .Publish(context => new OrchestrationOrderShippingRequestFailedEvent(context.Saga.OrderId, context.Message.Reason))
                     .Then(context =>
                     {
                         Console.WriteLine($"OrchestrationOrderShippingRequestFailedEvent After : {context.Saga}");
                     })
                    .TransitionTo(ShippingFailed)
            );

            SetCompletedWhenFinalized(); // Removes the completed tasks from the database when the state is finalized.
        }
    }
}
