using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Commands.CreateStockOut;

public class CreateStockOutCommand : IRequest<Result<Guid>>
{
    /// <summary>
    /// ID của phiếu soạn hàng đã hoàn thành.
    /// </summary>
    public Guid PickingNoteId { get; set; }
}
