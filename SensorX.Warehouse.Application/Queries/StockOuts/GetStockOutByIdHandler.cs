using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Application.Queries.StockOuts;

public class GetStockOutByIdHandler(
    IQueryBuilder<StockOut> _stockOutQueryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetStockOutByIdQuery, Result<StockOutDetailDto>>
{
    public async Task<Result<StockOutDetailDto>> Handle(GetStockOutByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _stockOutQueryBuilder.QueryAsNoTracking
                .Where(x => x.Id == new StockOutId(request.Id) && x.WarehouseId == new WarehouseId(request.WarehouseId));

            var result = await _queryExecutor.FirstOrDefaultAsync(
                query.Select(x => new StockOutDetailDto
                {
                    Id = x.Id.Value,
                    Code = x.Code.Value,
                    Description = x.Description,
                    PickingNoteId = x.PickingNoteId != null ? x.PickingNoteId.Value : null,
                    CreatedAt = x.CreatedAt,
                    Items = x.LineItems.Select(li => new StockOutItemDto
                    {
                        ProductId = li.ProductId.Value,
                        ProductCode = li.ProductCode.Value,
                        ProductName = li.ProductName,
                        Unit = li.Unit,
                        Quantity = li.Quantity.Value
                    }).ToList()
                }),
                cancellationToken);

            if (result == null)
                return Result<StockOutDetailDto>.Failure("Không tìm thấy phiếu xuất kho");

            return Result<StockOutDetailDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<StockOutDetailDto>.Failure(ex.Message);
        }
    }
}
