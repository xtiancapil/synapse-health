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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAlertService _alertService;

        public OrderService(
            ILogger logger,
            IOptions<Endpoints> endpoints,
            IHttpClientFactory httpClientFactory,
            IAlertService alertService)
        {
            _logger = logger;
            _endpoints = endpoints.Value;
            _httpClientFactory = httpClientFactory;
            _alertService = alertService;
        }

        public async Task<List<Order>> GetOrders()
        {
            using var httpClient = _httpClientFactory.CreateClient(_endpoints.OrdersUrl);            
            var response = await httpClient.GetAsync(_endpoints.OrdersUrl);
            if (response.IsSuccessStatusCode)
            {
                var ordersData = await response.Content.ReadFromJsonAsync<List<Order>>();
                return ordersData;
            }

            // Change this to a logging framework
            _logger.LogError(Constants.ORDER_RETRIEVE_ERROR);
            return null;

        }

        public async Task ProcessOrder(Order order)
        {
            foreach(var item in order.Items)
            {
                if(item.Status.Equals(Constants.DELIVERED, StringComparison.OrdinalIgnoreCase))
                {
                    // there should be some mechanism for success here
                    // how do we account for failed alerts ? do we reprocess them?
                    // Do we only alert when delivery notification is 0?
                    await _alertService.Alert(item, order.OrderId);
                    item.DeliveryNotification++;
                }
            }
        }

        public async Task UpdateOrder(Order order)
        {
            using var httpClient = _httpClientFactory.CreateClient(_endpoints.UpdatesUrl);
            var content = new StringContent(JsonConvert.SerializeObject(order), System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(_endpoints.UpdatesUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInfo($"Updated order sent for processing: OrderId {order.OrderId}");
            }
            else
            {
                _logger.LogError($"Failed to send updated order for processing: OrderId {order.OrderId}");
            }
        }
    }
}
