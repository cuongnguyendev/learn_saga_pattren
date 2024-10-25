namespace Shared.Infrastructure
{
    public static class RabbitQueueName
    {
        public const string StockReservedEventQueueName = "stock-reserved-queue";
        public const string StockOrderCreatedEventQueueName = "stock-order-created-queue";
        public const string StockPaymentFailedEventQueueName = "stock-payment-failed-queue";
        public const string OrderPaymentCompletedEventQueueName = "order-payment-completed-queue";
        public const string OrderPaymentFailedEventQueueName = "order-payment-failed-queue";
        public const string OrderStockNotReservedEventQueueName = "order-stock-not-reserved-queue";

        public const string OrderSaga = "order-saga-queue";
        public const string StockRollBackMessageQueueName = "stock-rollback-queue";
        public const string PaymentStockReservedRequestQueueName = "payment-stock-reserved-request-queue";
        public const string OrderRequestCompletedEventQueueName = "order-request-completed-queue";
        public const string OrderRequestFailedEventQueueName = "order-request-failed-queue";

        public const string OrderCancelRequestCompletedEventQueueName = "order-cancel-request-completed-queue";
        public const string OrderCancelRequestFailedEventQueueName = "order-cancel-request-failed-queue";
        public const string OrderCanceledRequestShippingQueueName = "order-cancel-request-shipping-queue";
        // Shipping-related queues
        public const string ShippingRequestQueueName = "shipping_request_queue";
        public const string ShippingCompletedEventQueueName = "shipping-completed-queue";
        public const string ShippingFailedEventQueueName = "shipping-failed-queue";
    }
}
