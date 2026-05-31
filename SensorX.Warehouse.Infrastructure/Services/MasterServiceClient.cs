using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Infrastructure.Services;

public class MasterServiceClient : IMasterServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MasterServiceClient> _logger;

    public MasterServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<MasterServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var baseUrl = configuration["ExternalServices:MasterApi:BaseUrl"] ?? "http://localhost:5202";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<OrderPaymentStatusDto?> GetOrderPaymentStatusAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/orders/{orderId}/payment-status", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Call to Master API payment-status failed with status code: {StatusCode}", response.StatusCode);
                return null;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<MasterApiResponse<OrderPaymentStatusDto>>(cancellationToken: cancellationToken);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Value != null)
            {
                return apiResponse.Value;
            }

            _logger.LogWarning("Call to Master API payment-status returned failure: {Message}", apiResponse?.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Master API for order payment status. OrderId: {OrderId}", orderId);
            return null;
        }
    }
}

public class MasterApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? Message { get; set; }
}
