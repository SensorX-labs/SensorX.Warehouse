using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Events;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Application.Common.Interfaces;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class SupplyRequestCreatedConsumer(
    IRepository<PickingNote> pickingNoteRepository,
    IUnitOfWork unitOfWork,
    ILogger<SupplyRequestCreatedConsumer> logger
) : IConsumer<SupplyRequestCreatedEvent>
{
    public async Task Consume(ConsumeContext<SupplyRequestCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Warehouse received SupplyRequestCreatedEvent: {SupplyRequestId}", message.SupplyRequestId);

        if (message.PickingNoteId != Guid.Empty)
        {
            var pickingNote = await pickingNoteRepository.GetByIdAsync(new PickingNoteId(message.PickingNoteId), context.CancellationToken);
            if (pickingNote != null)
            {
                await pickingNoteRepository.Update(pickingNote, context.CancellationToken);
                await unitOfWork.SaveChangesAsync(context.CancellationToken);
                logger.LogInformation("Processed SupplyRequestCreatedEvent for PickingNote {PickingNoteId}", message.PickingNoteId);
            }
        }
    }
}
