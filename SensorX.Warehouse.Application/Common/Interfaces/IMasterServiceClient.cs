namespace SensorX.Warehouse.Application.Common.Interfaces;

public interface IMasterServiceClient
{
    Task<OrderPaymentStatusDto?> GetOrderPaymentStatusAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public record OrderPaymentStatusDto(
    Guid OrderId,
    bool IsPaid,
    string PaymentStatus
);
