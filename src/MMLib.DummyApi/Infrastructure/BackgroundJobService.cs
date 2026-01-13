using System.Collections.Concurrent;
using MMLib.DummyApi.Configuration;
using MMLib.DummyApi.Domain.Orders;
using MMLib.DummyApi.Domain.Products;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MMLib.DummyApi.Infrastructure;

public class BackgroundJobService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DummyApiOptions _options;
    private readonly ConcurrentDictionary<Guid, BackgroundJobStatus> _productJobs = new();
    private readonly ConcurrentDictionary<Guid, BackgroundJobStatus> _orderJobs = new();
    private Timer? _timer;

    public BackgroundJobService(IServiceProvider serviceProvider, IOptions<DummyApiOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(ProcessJobs, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void ProcessJobs(object? state)
    {
        ProcessProductJobs();
        ProcessOrderJobs();
    }

    private void ProcessProductJobs()
    {
        var now = DateTime.UtcNow;
        var completedJobs = new List<Guid>();

        foreach (var (productId, job) in _productJobs)
        {
            if (now >= job.CompletedAt)
            {
                using var scope = _serviceProvider.CreateScope();
                var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
                var product = productService.GetById(productId);
                
                if (product != null)
                {
                    // Calculate price with 20% tax
                    var calculatedPrice = product.Price * 1.2m;
                    var updated = product with { CalculatedPrice = calculatedPrice };
                    
                    // Update via dataStore
                    var dataStore = scope.ServiceProvider.GetRequiredService<DataStore<Guid, Product>>();
                    dataStore.Update(productId, updated);
                }

                completedJobs.Add(productId);
            }
        }

        foreach (var id in completedJobs)
        {
            _productJobs.TryRemove(id, out _);
        }
    }

    private void ProcessOrderJobs()
    {
        var now = DateTime.UtcNow;
        var completedJobs = new List<Guid>();

        foreach (var (orderId, job) in _orderJobs)
        {
            if (now >= job.CompletedAt)
            {
                using var scope = _serviceProvider.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();
                
                // Get current order status
                var order = orderService.GetByIdForSystem(orderId);
                if (order != null)
                {
                    // Update status: Pending -> Processing -> Completed
                    var nextStatus = order.Status switch
                    {
                        OrderStatus.Pending => OrderStatus.Processing,
                        OrderStatus.Processing => OrderStatus.Completed,
                        _ => order.Status
                    };

                    if (nextStatus != order.Status)
                    {
                        orderService.UpdateStatus(orderId, nextStatus);
                        
                        // If not completed yet, schedule next update
                        if (nextStatus != OrderStatus.Completed)
                        {
                            var delayMs = _options.BackgroundJobDelayMs;
                            ScheduleOrderStatusUpdate(orderId, delayMs);
                        }
                    }
                }

                completedJobs.Add(orderId);
            }
        }

        foreach (var id in completedJobs)
        {
            _orderJobs.TryRemove(id, out _);
        }
    }

    public void ScheduleProductCalculation(Guid productId, int delayMs)
    {
        var completedAt = DateTime.UtcNow.AddMilliseconds(delayMs);
        _productJobs.TryAdd(productId, new BackgroundJobStatus(completedAt, "calculating"));
    }

    public string GetProductStatus(Guid productId)
    {
        if (_productJobs.ContainsKey(productId))
            return "processing";
        
        return "completed";
    }

    public void ScheduleOrderStatusUpdate(Guid orderId, int delayMs)
    {
        var completedAt = DateTime.UtcNow.AddMilliseconds(delayMs);
        _orderJobs.TryAdd(orderId, new BackgroundJobStatus(completedAt, "updating"));
    }

    public string GetOrderStatus(Guid orderId)
    {
        if (_orderJobs.ContainsKey(orderId))
            return "processing";
        
        return "completed";
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

public record BackgroundJobStatus(DateTime CompletedAt, string Status);
