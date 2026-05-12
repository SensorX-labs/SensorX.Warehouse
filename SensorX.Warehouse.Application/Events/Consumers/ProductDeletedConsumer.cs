using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Domain.AggregatesModel.ProductAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class ProductDeletedConsumer(
    IRepository<ProductReadModel> _productRepository,
    IUnitOfWork _unitOfWork,
    ILogger<ProductDeletedConsumer> _logger
) : IConsumer<ProductDeletedEvent>
{
    public async Task Consume(ConsumeContext<ProductDeletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming ProductDeletedEvent for ProductId: {ProductId}", message.ProductId);

        var productId = new ProductId(message.ProductId);
        var product = await _productRepository.GetByIdAsync(productId, context.CancellationToken);

        if (product != null)
        {
            await _productRepository.Delete(product, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Successfully deleted ProductReadModel for ProductId: {ProductId}", message.ProductId);
        }
    }
}
