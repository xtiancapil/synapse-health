using DeliveryNotifier.Models;
using DeliveryNotifier.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using Serilog;

namespace DeliveryNotifier
{
    public static class Program
    {
        
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo
                    .Console()
                    .CreateLogger();

            var host = BuildHost(args);
            host.Run();            
        }

        public static IHost BuildHost(string[] args) =>
            Host.CreateDefaultBuilder()
            
            .ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IHostedService, DeliveryNotifierService>();
                services.AddScoped<DeliveryNotifier.Interfaces.ILogger, Logger>();
                services.AddScoped<IOrderService, OrderService>();
                services.AddScoped<IAlertService, AlertService>();
                services.AddHttpClient<IOrderService, OrderService>()
                    .AddPolicyHandler(GetRetryPolicy());
                services.AddHttpClient<IAlertService, AlertService>()
                    .AddPolicyHandler(GetRetryPolicy());


                services.Configure<Endpoints>(ctx.Configuration.GetSection("Endpoints"));
            })
            .UseSerilog()
            .Build();

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}