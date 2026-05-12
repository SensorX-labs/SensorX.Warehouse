using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Domain.AggregatesModel.ProductAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class ProductSyncConsumer(
    IRepository<ProductReadModel> _productRepository,
    IUnitOfWork _unitOfWork,
    ILogger<ProductSyncConsumer> _logger
) : IConsumer<ProductSyncEvent>
{
    public async Task Consume(ConsumeContext<ProductSyncEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming ProductSyncEvent for ProductId: {ProductId}, Code: {Code}", message.ProductId, message.Code);

        var productId = new ProductId(message.ProductId);
        var product = await _productRepository.GetByIdAsync(productId, context.CancellationToken);

        if (product == null)
        {
            _logger.LogInformation("Creating new ProductReadModel for ProductId: {ProductId}", message.ProductId);
            product = new ProductReadModel(
                productId,
                message.Code,
                message.Name,
                message.Unit,
                message.Manufacture,
                message.Status
            );
            await _productRepository.Add(product, context.CancellationToken);
        }
        else
        {
            _logger.LogInformation("Updating existing ProductReadModel for ProductId: {ProductId}", message.ProductId);
            product.Update(
                message.Code,
                message.Name,
                message.Unit,
                message.Manufacture,
                message.Status
            );
            await _productRepository.Update(product, context.CancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Successfully synced product: {Code}", message.Code);
    }
}
