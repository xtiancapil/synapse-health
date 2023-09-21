using DeliveryNotifier.Interfaces;
using DeliveryNotifier.Services;
using Microsoft.Extensions.Hosting;

namespace DeliveryNotifier
{
    public class DeliveryNotifierService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;

        public DeliveryNotifierService(ILogger logger,
            IOrderService orderService) {
            _logger = logger;
            _orderService = orderService;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("running!");

            var orders = await _orderService.GetOrders();
            foreach (var order in orders)
            {                
                await _orderService.ProcessOrder(order);
                await _orderService.UpdateOrder(order);
            }

            _logger.LogWarning("done!");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("stopping!");
            return Task.CompletedTask;
        }
    }
}
