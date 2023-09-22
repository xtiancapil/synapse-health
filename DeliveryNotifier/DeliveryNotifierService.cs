using DeliveryNotifier.Interfaces;
using DeliveryNotifier.Services;
using Microsoft.Extensions.Hosting;

namespace DeliveryNotifier
{
    public class DeliveryNotifierService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifeTime;
        private readonly IOrderService _orderService;

        public DeliveryNotifierService(ILogger logger,
            IHostApplicationLifetime appLifetime,
            IOrderService orderService) {
            _logger = logger;
            _appLifeTime = appLifetime;
            _orderService = orderService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning("Retrieving orders");

                var orders = await _orderService.GetOrders();
                await Parallel.ForEachAsync(orders, async (order, cts) =>
                {
                    await _orderService.ProcessOrder(order);
                    await _orderService.UpdateOrder(order);
                });

                _logger.LogWarning("Finished processing orders. Sleeping for 5 seconds.");
                await Task.Delay(5000);
            }
        }        
    }
}
