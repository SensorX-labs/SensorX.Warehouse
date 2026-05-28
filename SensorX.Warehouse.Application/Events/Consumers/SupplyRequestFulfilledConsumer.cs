using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Events;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Application.Common.Interfaces;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class SupplyRequestFulfilledConsumer(
    IRepository<PickingNote> pickingNoteRepository,
    IUnitOfWork unitOfWork,
    ILogger<SupplyRequestFulfilledConsumer> logger
) : IConsumer<SupplyRequestFulfilledEvent>
{
    public async Task Consume(ConsumeContext<SupplyRequestFulfilledEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Warehouse received SupplyRequestFulfilledEvent: {SupplyRequestId} for PickingNote: {PickingNoteId}", message.SupplyRequestId, message.PickingNoteId);

        var pickingNote = await pickingNoteRepository.GetByIdAsync(new PickingNoteId(message.PickingNoteId), context.CancellationToken);
        if (pickingNote != null)
        {
            pickingNote.SetLinkedSupplyRequest(message.SupplyRequestId);
            await pickingNoteRepository.Update(pickingNote, context.CancellationToken);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("Linked SupplyRequest {SupplyRequestId} to PickingNote {PickingNoteId}", message.SupplyRequestId, message.PickingNoteId);
        }
    }
}
