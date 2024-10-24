// File: OrderStateMachine.cs
namespace StateMachineWorkerService.CustomState
{
    using MassTransit;
    using MassTransit.Internals;
    using Newtonsoft.Json;
    using Shared.Orchestration;
    using StateMachineWorkerService.Models;
    using System;
    using System.Text.Json;

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
        public Event<IOrchestrationOrderCancellationEvent> OrderCancellationEvent { get; set; }



        // States
        public State OrderCreated { get; set; }
        public State Cancelled { get; set; }
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
            Event(() => OrderCancellationEvent, eventCorrelationConfigurator =>
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
                        context.Saga.OrderItems= System.Text.Json.JsonSerializer.Serialize(context.Message.OrderItems);
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
                    .Publish(context => new OrchestrationOrderShippingRequestCompletedEvent(context.Saga.OrderId))
                    .TransitionTo(ShippingCompleted)
                   .Finalize(),

                When(ShippingFailedEvent)
                    .Publish(context => new OrchestrationOrderShippingRequestFailedEvent(context.Saga.OrderId, context.Message.Reason))
                    .Send(new Uri($"queue:{RabbitQueueName.StockRollBackMessageQueueName}"), context => new OrchestrationStockRollBackMessage(System.Text.Json.JsonSerializer.Deserialize<List<OrderItemMessage>>(context.Saga.OrderItems)))
                    .TransitionTo(ShippingFailed)
            );
            DuringAny(
    When(OrderCancellationEvent)
        .IfElse(context => context.Saga.CurrentState == "ShippingCompleted" || context.Saga.CurrentState == "ShippingFailed" || context.Saga.CurrentState == "Cancelled",
            shippingContext => shippingContext
                .Then(context =>
                {
                    // Optionally: Log or notify that the cancellation cannot proceed
                    Console.WriteLine($"Cancellation denied: Order {context.Saga.OrderId} has already been shipped.");
                }),
            cancellableContext => cancellableContext
                // Proceed with normal cancellation logic
                .IfElse(context => context.Saga.CurrentState == "StockReserved",
                    reservedContext => reservedContext
                        .Send(new Uri($"queue:{RabbitQueueName.StockRollBackMessageQueueName}"), reservedContext => new OrchestrationStockRollBackMessage(System.Text.Json.JsonSerializer.Deserialize<List<OrderItemMessage>>(reservedContext.Saga.OrderItems)))
                         //.Publish(reservedContext => new OrchestrationOrderCancelledEvent(reservedContext.Saga.OrderId))
                         .Then(context =>
                         {
                             // Optionally: Log or notify that the cancellation cannot proceed
                             Console.WriteLine($"Cancellation denied: Order {context.Saga.OrderId} has already been shipped.");
                         })
                        .TransitionTo(Cancelled),
                    paymentContext => paymentContext.IfElse(
                        paymentCompletedContext => paymentCompletedContext.Saga.CurrentState == "PaymentCompleted",
                        paymentCompletedContext => paymentCompletedContext
                             // .Send(new Uri($"queue:{RabbitQueueName.PaymentRollBackQueueName}"), paymentCompletedContext => new OrchestrationPaymentRollBackMessage(paymentCompletedContext.Saga.OrderId))
                             // .Publish(paymentCompletedContext => new OrchestrationOrderCancelledEvent(paymentCompletedContext.Saga.OrderId))
                             .Then(context =>
                             {
                                 // Optionally: Log or notify that the cancellation cannot proceed
                                 Console.WriteLine($"Cancellation denied: Order {context.Saga.OrderId} has already been shipped.");
                             })
                            .TransitionTo(Cancelled),
                        otherContext => otherContext
                             // .Publish(otherContext => new OrchestrationOrderCancelledEvent(otherContext.Saga.OrderId))
                             .Then(context =>
                             {
                                 // Optionally: Log or notify that the cancellation cannot proceed
                                 Console.WriteLine($"Cancellation denied: Order {context.Saga.OrderId} has already been shipped.");
                             })
                            .TransitionTo(Cancelled)
                    )
                )
        )
);


            SetCompletedWhenFinalized(); // Removes the completed tasks from the database when the state is finalized.
        }
    }

}
