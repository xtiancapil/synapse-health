using DeliveryNotifier.Interfaces;
using DeliveryNotifier.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DeliveryNotifier.Services
{
    public interface IAlertService
    {
        public Task<bool> Alert(OrderItem item, Guid orderId);
    }

    public class AlertService : IAlertService
    {
        private readonly ILogger _logger;
        private readonly Endpoints _endpoints;
        private readonly HttpClient _client;

        public AlertService(
            ILogger logger,
            IOptions<Endpoints> endpoints,
            HttpClient client)
        {
            _logger = logger;
            _endpoints = endpoints.Value;
            _client = client;
        }

        public async Task<bool> Alert(OrderItem item, Guid orderId)
        {
            var alertData = new
            {
                Message = $"Alert for delivered item: Order {orderId}, Item: {item.Description}, " +
                          $"Delivery Notifications: {item.DeliveryNotification}"
            };
            var content = new StringContent(JsonConvert.SerializeObject(item), System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(_endpoints.NotificationsUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Alert sent for delivered item: {item.Description}");
                return true;
            }
            else
            {
                _logger.LogError($"Failed to send alert for delivered item: {item.Description}");
                return false;
            }
        }
    }
}
