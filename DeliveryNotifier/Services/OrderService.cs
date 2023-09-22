using DeliveryNotifier.Interfaces;
using DeliveryNotifier.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace DeliveryNotifier.Services
{
    public interface IOrderService
    {
        public Task<List<Order>> GetOrders();
        public Task UpdateOrder(Order order);
        public Task ProcessOrder(Order order);
    }

    public class OrderService : IOrderService
    {
        private readonly ILogger _logger;
        private readonly Endpoints _endpoints;
        private readonly HttpClient _client;
        private readonly IAlertService _alertService;

        public OrderService(
            ILogger logger,
            IOptions<Endpoints> endpoints,
            HttpClient client,
            IAlertService alertService)
        {
            _logger = logger;
            _endpoints = endpoints.Value;
            _client = client;
            _alertService = alertService;
        }

        public async Task<List<Order>> GetOrders()
        {
            var response = await _client.GetAsync(_endpoints.OrdersUrl);
            if (response.IsSuccessStatusCode)
            {
                var ordersData = await response.Content.ReadFromJsonAsync<List<Order>>();
                return ordersData;
            }

            _logger.LogError(Constants.ORDER_RETRIEVE_ERROR);
            return null;

        }

        public async Task ProcessOrder(Order order)
        {
            var errors = 0;
            foreach (var item in order.Items?.Where(x => Constants.DELIVERED.Equals(x.Status, StringComparison.OrdinalIgnoreCase)))
            {
                    // Keep track of the number of errors encountered when trying to alert.                    
                    if(await _alertService.Alert(item, order.OrderId))
                        item.DeliveryNotification++;
                    else errors++;
            }

            if(errors > 0)
            {
                // What should we do for failed attempts to send an alert?
                // Do we want to re-process the entire order? 
                _logger.LogError($"Failed to send alerts for some items: OrderId: {order.OrderId}");                
            }
        }

        public async Task UpdateOrder(Order order)
        {
            var content = new StringContent(JsonConvert.SerializeObject(order), System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(_endpoints.UpdatesUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Updated order sent for processing: OrderId {order.OrderId}");
            }
            else
            {
                // TODO: Add queue
                _logger.LogError($"Failed to send updated order for processing: OrderId {order.OrderId}");
            }
        }
    }
}
