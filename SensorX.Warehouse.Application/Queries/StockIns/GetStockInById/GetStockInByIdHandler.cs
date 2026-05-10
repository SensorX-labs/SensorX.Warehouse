using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate.Specifications;
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Application.Queries.StockIns;

public class GetStockInByIdHandler(
    IRepository<StockIn> _stockInRepository
) : IRequestHandler<GetStockInByIdQuery, Result<StockInDetailDto>>
{
    public async Task<Result<StockInDetailDto>> Handle(GetStockInByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var spec = new GetStockInById(new StockInId(request.Id));
            var stockIn = await _stockInRepository.FirstOrDefaultAsync(spec, cancellationToken);

            if (stockIn is null)
                return Result<StockInDetailDto>.Failure("Stock in not found");

            Console.WriteLine($"[DEBUG] StockIn {stockIn.Id.Value} loaded. LineItems count: {stockIn.LineItems.Count}");

            var result = new StockInDetailDto(
                stockIn.Id.Value,
                stockIn.Code.Value,
                stockIn.TransferOrderCode?.Value,
                stockIn.Description,
                stockIn.ReceivedDate,
                stockIn.CreatedBy,
                stockIn.DeliveredBy,
                stockIn.WarehouseKeeper,
                stockIn.LineItems.Select(i => new StockInItemDto(
                    i.ProductId.Value,
                    i.ProductName,
                    i.ProductCode.Value,
                    i.Unit,
                    (int)i.Quantity.Value
                )).ToList()
            );

            return Result<StockInDetailDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<StockInDetailDto>.Failure(ex.Message);
        }
    }
}
