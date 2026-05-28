using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Events;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Application.Common.Interfaces;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class TransferOrderFinishedConsumer(
    IRepository<PickingNote> pickingNoteRepository,
    IUnitOfWork unitOfWork,
    ILogger<TransferOrderFinishedConsumer> logger
) : IConsumer<TransferOrderFinishedEvent>
{
    public async Task Consume(ConsumeContext<TransferOrderFinishedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Warehouse received TransferOrderFinishedEvent: {TransferOrderId} for PickingNote: {PickingNoteId}", message.TransferOrderId, message.PickingNoteId);

        var pickingNote = await pickingNoteRepository.GetByIdAsync(new PickingNoteId(message.PickingNoteId), context.CancellationToken);
        if (pickingNote != null)
        {
            pickingNote.SetLinkedTransferOrder(message.TransferOrderId);
            await pickingNoteRepository.Update(pickingNote, context.CancellationToken);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("Linked TransferOrder {TransferOrderId} to PickingNote {PickingNoteId}", message.TransferOrderId, message.PickingNoteId);
        }
    }
}
