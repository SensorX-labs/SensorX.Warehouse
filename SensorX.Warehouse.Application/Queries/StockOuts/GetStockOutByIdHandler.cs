using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Application.Queries.StockOuts;

public class GetStockOutByIdHandler(
    IQueryBuilder<StockOut> _stockOutQueryBuilder,
    IQueryExecutor _queryExecutor,
    IRepository<PickingNote> _pickingNoteRepository
) : IRequestHandler<GetStockOutByIdQuery, Result<StockOutDetailDto>>
{
    public async Task<Result<StockOutDetailDto>> Handle(GetStockOutByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _stockOutQueryBuilder.QueryAsNoTracking
                .Where(x => x.Id == new StockOutId(request.Id) && x.WarehouseId == new WarehouseId(request.WarehouseId));

            var stockOut = await _queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
            if (stockOut == null)
                return Result<StockOutDetailDto>.Failure("Không tìm thấy phiếu xuất kho");

            string? transferOrderCode = null;
            if (stockOut.PickingNoteId != null)
            {
                var pickingNote = await _pickingNoteRepository.GetByIdAsync(stockOut.PickingNoteId, cancellationToken);
                if (pickingNote != null && pickingNote.SourceDocument.Type == DocumentType.TransferOrder)
                {
                    transferOrderCode = pickingNote.SourceDocument.Code;
                }
            }

            var result = new StockOutDetailDto
            {
                Id = stockOut.Id.Value,
                Code = stockOut.Code.Value,
                Description = stockOut.Description,
                PickingNoteId = stockOut.PickingNoteId?.Value,
                TransferOrderCode = transferOrderCode,
                CreatedAt = stockOut.CreatedAt,
                Items = stockOut.LineItems.Select(li => {
                    var noteStr = li.Note ?? string.Empty;
                    double adjQty = li.Quantity.Value;
                    string cleanNote = noteStr;
                    
                    if (noteStr.StartsWith("[Adj:"))
                    {
                        var endIdx = noteStr.IndexOf(']');
                        if (endIdx > 0)
                        {
                            var numStr = noteStr.Substring(5, endIdx - 5);
                            if (double.TryParse(numStr, out var parsedQty))
                            {
                                adjQty = parsedQty;
                                cleanNote = noteStr.Substring(endIdx + 1).Trim();
                            }
                        }
                    }

                    return new StockOutItemDto
                    {
                        ProductId = li.ProductId.Value,
                        ProductCode = li.ProductCode.Value,
                        ProductName = li.ProductName,
                        Unit = li.Unit,
                        Quantity = li.Quantity.Value,
                        AdjustedQuantity = adjQty,
                        Note = string.IsNullOrEmpty(cleanNote) ? null : cleanNote
                    };
                }).ToList()
            };

            return Result<StockOutDetailDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<StockOutDetailDto>.Failure(ex.Message);
        }
    }
}
