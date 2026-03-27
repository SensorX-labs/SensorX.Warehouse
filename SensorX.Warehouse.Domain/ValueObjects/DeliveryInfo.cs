namespace SensorX.Warehouse.Domain.ValueObjects;

public record DeliveryInfo(
    string ReceiverName,
    string ReceiverPhone,
    string DeliveryAddress,
    string CompanyName,
    string TaxCode
);