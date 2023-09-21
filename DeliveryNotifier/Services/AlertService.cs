using DeliveryNotifier.Interfaces;
using DeliveryNotifier.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DeliveryNotifier.Services
{
    public interface IAlertService
    {
        public Task Alert(OrderItem item, Guid orderId);
    }

    public class AlertService : IAlertService
    {
        private readonly ILogger _logger;
        private readonly Endpoints _endpoints;
        private readonly IHttpClientFactory _httpClientFactory;

        public AlertService(
            ILogger logger,
            IOptions<Endpoints> endpoints,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _endpoints = endpoints.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task Alert(OrderItem item, Guid orderId)
        {
            using var httpClient = _httpClientFactory.CreateClient(_endpoints.NotificationsUrl);
            var alertData = new
            {
                Message = $"Alert for delivered item: Order {orderId}, Item: {item.Description}, " +
                          $"Delivery Notifications: {item.DeliveryNotification}"
            };
            var content = new StringContent(JsonConvert.SerializeObject(item), System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(_endpoints.NotificationsUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInfo($"Alert sent for delivered item: {item.Description}");
            }
            else
            {
                _logger.LogError($"Failed to send alert for delivered item: {item.Description}");
            }
        }
    }
}
