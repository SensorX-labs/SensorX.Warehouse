using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Domain.AggregatesModel.ProductAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class DeleteProductConsumer(
    IRepository<ProductReadModel> _productRepository,
    IUnitOfWork _unitOfWork,
    ILogger<DeleteProductConsumer> _logger
) : IConsumer<DeleteProductEvent>
{
    public async Task Consume(ConsumeContext<DeleteProductEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming DeleteProductEvent for ProductId: {ProductId}", message.Id);

        var productId = new ProductId(message.Id);
        var product = await _productRepository.GetByIdAsync(productId, context.CancellationToken);

        if (product != null)
        {
            await _productRepository.Delete(product, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Successfully deleted ProductReadModel for ProductId: {ProductId}", message.Id);
        }
        else
        {
            _logger.LogWarning("ProductReadModel not found for ProductId: {ProductId} during DeleteProductEvent", message.Id);
        }
    }
}
