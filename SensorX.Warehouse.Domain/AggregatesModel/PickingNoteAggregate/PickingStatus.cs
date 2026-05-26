namespace SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

public enum PickingStatus
{
    Pending,   // Chờ soạn
    Picking,   // Đang soạn
    Completed, // Xác nhận đủ hàng
    Canceled,  // Đã hủy
    WaitingTransfer, // Đang chờ điều chuyển kho
    WaitingSupply    // Đang chờ cung ứng
}
