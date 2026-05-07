using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Commands.ApproveStockAdjustment;

public class ApproveStockAdjustmentCommand : IRequest<Result<Guid>>
{
    public Guid Id { get; set; }
}