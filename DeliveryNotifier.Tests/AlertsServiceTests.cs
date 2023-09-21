using DeliveryNotifier.Interfaces;
using DeliveryNotifier.Models;
using DeliveryNotifier.Services;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;
using System.Net;

namespace DeliveryNotifier.Tests
{
    public class AlertsServiceTests
    {
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly MockHttpMessageHandler _mockedHandler = new();

        private readonly IAlertService _sut;
        private readonly Endpoints _opts;
        public AlertsServiceTests()
        {
            _opts = new Endpoints()
            {
                OrdersUrl = "https://orders-api/orders",
                NotificationsUrl = "https://alert-api.com/alerts",
                UpdatesUrl = "https://update-api.com/update"
            };

            _sut = new AlertService(
                _loggerMock.Object,
                Options.Create<Endpoints>(_opts),
                _httpClientFactoryMock.Object);
        }

        [Fact(DisplayName = "Alert should call LogInfo on successful publish of an alert")]
        public async Task Test1()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var orderItem = new OrderItem
            {
                Description = "Test",
                Status = "Delivered"
            };

            _mockedHandler.When(_opts.NotificationsUrl)
                .Respond(statusCode: HttpStatusCode.OK);

            _httpClientFactoryMock.Setup(x => x.CreateClient(_opts.NotificationsUrl))
                .Returns(new HttpClient(_mockedHandler)
                {
                    BaseAddress = new Uri(_opts.NotificationsUrl)
                });

            _loggerMock.Setup(x => x.LogInfo(It.IsAny<string>()));

            // Act
            await _sut.Alert(orderItem, guid);

            // Assert
            _loggerMock.Verify(x => x.LogInfo(It.IsAny<string>()), Times.Once());
            _loggerMock.Verify(x => x.LogError(It.IsAny<string>()), Times.Never());
        }

        [Fact(DisplayName = "Alert should call LogError on unsuccessful publish for Alert")]
        public async Task Test2()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var orderItem = new OrderItem
            {
                Description = "Test",
                Status = "Delivered"
            };

            _mockedHandler.When(_opts.NotificationsUrl)
                .Respond(statusCode: HttpStatusCode.InternalServerError);

            _httpClientFactoryMock.Setup(x => x.CreateClient(_opts.NotificationsUrl))
                .Returns(new HttpClient(_mockedHandler)
                {
                    BaseAddress = new Uri(_opts.NotificationsUrl)
                });

            _loggerMock.Setup(x => x.LogError(It.IsAny<string>()));

            // Act
            await _sut.Alert(orderItem, guid);

            // Assert
            _loggerMock.Verify(x => x.LogInfo(It.IsAny<string>()), Times.Never());
            _loggerMock.Verify(x => x.LogError(It.IsAny<string>()), Times.Once());
        }
    }
}
