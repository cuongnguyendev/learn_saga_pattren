using StateMachineWorkerService.BackgroudService;

namespace StateMachineWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var shippingRequestService = scope.ServiceProvider.GetRequiredService<ShippingRequestService>();
                    await shippingRequestService.CheckAndCallShippingRequest(stoppingToken);
                }
                // Wait for 2 minutes before checking again
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }
    }
}
