using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Common.Interfaces;

namespace SensorX.Warehouse.Infrastructure.Services;

public class DataServiceClient : IDataServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DataServiceClient> _logger;

    public DataServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<DataServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var baseUrl = configuration["ExternalServices:DataApi:BaseUrl"] ?? "http://localhost:5200";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<List<WarehouseProductContextDto>> GetProductPricingContextAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/catalog/products/warehouse-pricing-context", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Call to Data API warehouse-pricing-context failed with status code: {StatusCode}", response.StatusCode);
                return [];
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<DataApiResponse<List<WarehouseProductContextDto>>>(cancellationToken: cancellationToken);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Value != null)
            {
                return apiResponse.Value;
            }

            _logger.LogWarning("Call to Data API warehouse-pricing-context returned failure: {Message}", apiResponse?.Message);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Data API for warehouse pricing context.");
            return [];
        }
    }
}

public class DataApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? Message { get; set; }
}
