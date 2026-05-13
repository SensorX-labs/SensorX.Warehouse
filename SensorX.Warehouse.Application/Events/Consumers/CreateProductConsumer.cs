using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Domain.AggregatesModel.ProductAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class CreateProductConsumer(
    IRepository<ProductReadModel> _productRepository,
    IUnitOfWork _unitOfWork,
    ILogger<CreateProductConsumer> _logger
) : IConsumer<CreateProductEvent>
{
    public async Task Consume(ConsumeContext<CreateProductEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming CreateProductEvent for ProductId: {ProductId}, Code: {Code}", message.Id, message.Code);

        var productId = new ProductId(message.Id);
        var product = await _productRepository.GetByIdAsync(productId, context.CancellationToken);

        if (product == null)
        {
            _logger.LogInformation("Creating new ProductReadModel for ProductId: {ProductId}", message.Id);
            product = new ProductReadModel(
                productId,
                message.Code,
                message.Name,
                message.Unit,
                message.Manufacture,
                message.Status.ToString()
            );
            await _productRepository.Add(product, context.CancellationToken);
        }
        else
        {
            _logger.LogInformation("Updating existing ProductReadModel for ProductId: {ProductId}", message.Id);
            product.Update(
                message.Code,
                message.Name,
                message.Unit,
                message.Manufacture,
                message.Status.ToString()
            );
            await _productRepository.Update(product, context.CancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Successfully processed CreateProductEvent for product: {Code}", message.Code);
    }
}
