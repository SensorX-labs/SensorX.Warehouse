using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Domain.AggregatesModel.ProductAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class ChangeProductStatusConsumer(
    IRepository<ProductReadModel> _productRepository,
    IUnitOfWork _unitOfWork,
    ILogger<ChangeProductStatusConsumer> _logger
) : IConsumer<ChangeProductStatusEvent>
{
    public async Task Consume(ConsumeContext<ChangeProductStatusEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming ChangeProductStatusEvent for ProductId: {ProductId}, Status: {Status}", message.Id, message.Status);

        var productId = new ProductId(message.Id);
        var product = await _productRepository.GetByIdAsync(productId, context.CancellationToken);

        if (product != null)
        {
            product.Update(
                product.Code,
                product.Name,
                product.Unit,
                product.Manufacture,
                message.Status.ToString()
            );
            await _productRepository.Update(product, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Successfully updated status of ProductReadModel for ProductId: {ProductId}", message.Id);
        }
        else
        {
            _logger.LogWarning("ProductReadModel not found for ProductId: {ProductId} during ChangeProductStatusEvent", message.Id);
        }
    }
}
