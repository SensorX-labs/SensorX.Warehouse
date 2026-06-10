namespace SensorX.Warehouse.Application.Common.Interfaces;

public interface IDataServiceClient
{
    Task<List<WarehouseProductContextDto>> GetProductPricingContextAsync(CancellationToken cancellationToken = default);
}

public record WarehouseProductContextDto(
    Guid ProductId,
    string CategoryName,
    decimal CurrentPrice
);
