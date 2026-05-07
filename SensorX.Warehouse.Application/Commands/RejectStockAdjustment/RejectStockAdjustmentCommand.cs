using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Commands.RejectStockAdjustment;

public class RejectStockAdjustmentCommand : IRequest<Result<Guid>>
{
    public Guid Id { get; set; }
    public required string Reason { get; set; }
}