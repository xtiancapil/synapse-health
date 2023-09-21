using DeliveryNotifier.Interfaces;
using DeliveryNotifier.Models;
using DeliveryNotifier.Services;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using System.Net;

namespace DeliveryNotifier.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly Mock<IAlertService> _alertServiceMock = new();
        private readonly MockHttpMessageHandler _mockedHandler = new();

        private readonly IOrderService _sut;
        private readonly Endpoints _opts;
        public OrderServiceTests()
        {
            _opts = new Endpoints()
            {
                OrdersUrl = "https://orders-api/orders",
                NotificationsUrl = "https://alert-api.com/alerts",
                UpdatesUrl = "https://update-api.com/update"
            };

            _sut = new OrderService(
                _loggerMock.Object,
                Options.Create<Endpoints>(_opts),
                _httpClientFactoryMock.Object,
                _alertServiceMock.Object);
        }

        [Fact(DisplayName = "Should return null when status code is not 200.")]
        public async Task Test1()
        {
            // Arrange
            _mockedHandler.When(_opts.OrdersUrl)
                .Respond(HttpStatusCode.BadRequest);

            _httpClientFactoryMock.Setup(x => x.CreateClient(_opts.OrdersUrl))
                .Returns(new HttpClient(_mockedHandler)
                {
                    BaseAddress = new Uri(_opts.OrdersUrl)
                });


            // Act
            var orders = await _sut.GetOrders();
            // Assert
            Assert.Null(orders);
        }

        [Fact(DisplayName = "Should return the list of orders when it is a successful request.")]
        public async Task Test2()
        {
            var guid = Guid.NewGuid();
            var orders = new List<Order>() {
                new Order() {
                    OrderId = guid,
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Description = "Test",
                            Status = "Pending"
                        }
                    }
                }
            };

            // Arrange
            _mockedHandler.When(_opts.OrdersUrl)
                .Respond("application/json", JsonConvert.SerializeObject(orders));

            _httpClientFactoryMock.Setup(x => x.CreateClient(_opts.OrdersUrl))
                .Returns(new HttpClient(_mockedHandler)
                {
                    BaseAddress = new Uri(_opts.OrdersUrl)
                });

            // Act
            var retrievedOrders = await _sut.GetOrders();
            // Assert
            Assert.NotEmpty(retrievedOrders);
            Assert.Collection<Order>(retrievedOrders, _ =>
            {
                Assert.Equal(_.OrderId, guid);
            });
        }

        [Fact(DisplayName = "ProcessOrder should increment delivery notification by 1 after a successful alert")]
        public async Task Test3()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var order = new Order()
            {
                OrderId = guid,
                Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Description = "Test",
                            Status = "Delivered"
                        },
                        new OrderItem
                        {
                            Description = "Not Delivered",
                            Status = "Pending"
                        },
                    }
            };

            // Arrange
            _alertServiceMock.Setup(x => x.Alert(It.IsAny<OrderItem>(), It.IsAny<Guid>()))
                 .Returns(Task.CompletedTask);

            // Act
            await _sut.ProcessOrder(order);

            // Assert
            _alertServiceMock.Verify(x => x.Alert(It.IsAny<OrderItem>(), It.IsAny<Guid>()), Times.Once());

            Assert.Collection<OrderItem>(order.Items, _ =>
            {
                Assert.Equal(1, _.DeliveryNotification);
                Assert.True(Constants.DELIVERED.Equals(_.Status, StringComparison.OrdinalIgnoreCase));

            },
            _ =>
            {
                Assert.Equal(0, _.DeliveryNotification);
                Assert.False(Constants.DELIVERED.Equals(_.Status, StringComparison.OrdinalIgnoreCase));
            });
        }

        [Fact(DisplayName = "UpdateOrder should call LogInfo on successful update for Order")]
        public async Task Test4()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var order = new Order()
            {
                OrderId = guid,
                Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Description = "Test",
                            Status = "Delivered"
                        },
                        new OrderItem
                        {
                            Description = "Not Delivered",
                            Status = "Pending"
                        },
                    }
            };

            _mockedHandler.When(_opts.UpdatesUrl)
                .Respond(statusCode: HttpStatusCode.OK);

            _httpClientFactoryMock.Setup(x => x.CreateClient(_opts.UpdatesUrl))
                .Returns(new HttpClient(_mockedHandler)
                {
                    BaseAddress = new Uri(_opts.UpdatesUrl)
                });

            _loggerMock.Setup(x => x.LogInfo(It.IsAny<string>()));
            
            // Act
            await _sut.UpdateOrder(order);

            // Assert
            _loggerMock.Verify(x => x.LogInfo(It.IsAny<string>()), Times.Once());
            _loggerMock.Verify(x => x.LogError(It.IsAny<string>()), Times.Never());
        }

        [Fact(DisplayName = "UpdateOrder should call LogError on unsuccessful update for Order")]
        public async Task Test5()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var order = new Order()
            {
                OrderId = guid,
                Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Description = "Test",
                            Status = "Delivered"
                        },
                        new OrderItem
                        {
                            Description = "Not Delivered",
                            Status = "Pending"
                        },
                    }
            };

            _mockedHandler.When(_opts.UpdatesUrl)
                .Respond(statusCode: HttpStatusCode.InternalServerError);

            _httpClientFactoryMock.Setup(x => x.CreateClient(_opts.UpdatesUrl))
                .Returns(new HttpClient(_mockedHandler)
                {
                    BaseAddress = new Uri(_opts.UpdatesUrl)
                });

            _loggerMock.Setup(x => x.LogError(It.IsAny<string>()));

            // Act
            await _sut.UpdateOrder(order);

            // Assert
            _loggerMock.Verify(x => x.LogInfo(It.IsAny<string>()), Times.Never());
            _loggerMock.Verify(x => x.LogError(It.IsAny<string>()), Times.Once());
        }
    }
}