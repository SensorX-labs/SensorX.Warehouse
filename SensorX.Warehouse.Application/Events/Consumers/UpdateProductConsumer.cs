using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Domain.AggregatesModel.ProductAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class UpdateProductConsumer(
    IRepository<ProductReadModel> _productRepository,
    IUnitOfWork _unitOfWork,
    ILogger<UpdateProductConsumer> _logger
) : IConsumer<UpdateProductEvent>
{
    public async Task Consume(ConsumeContext<UpdateProductEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming UpdateProductEvent for ProductId: {ProductId}", message.Id);

        var productId = new ProductId(message.Id);
        var product = await _productRepository.GetByIdAsync(productId, context.CancellationToken);

        if (product != null)
        {
            product.Update(
                product.Code,
                message.Name,
                message.Unit,
                message.Manufacture,
                product.Status
            );
            await _productRepository.Update(product, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Successfully updated ProductReadModel for ProductId: {ProductId}", message.Id);
        }
        else
        {
            _logger.LogWarning("ProductReadModel not found for ProductId: {ProductId} during UpdateProductEvent", message.Id);
        }
    }
}
