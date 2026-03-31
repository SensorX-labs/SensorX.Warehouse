using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Infrastructure.EntityConfigurations;

public class StockOutConfiguration : IEntityTypeConfiguration<StockOut>
{
    public void Configure(EntityTypeBuilder<StockOut> builder)
    {
        builder.ToTable("StockOuts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new StockOutId(x))
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => Code.From(x));

        builder.Property(x => x.WarehouseId)
            .HasConversion(x => x.Value, x => new WarehouseId(x));

        builder.Property(x => x.PickingNoteId)
            .HasConversion(x => x!.Value, x => new PickingNoteId(x))
            .IsRequired(false);

        builder.OwnsOne(x => x.DeliveryInfo, deliveryInfo =>
        {
            deliveryInfo.Property(x => x.ReceiverName).HasColumnName("ReceiverName");
            deliveryInfo.Property(x => x.ReceiverPhone).HasColumnName("ReceiverPhone");
            deliveryInfo.Property(x => x.DeliveryAddress).HasColumnName("DeliveryAddress");
            deliveryInfo.Property(x => x.CompanyName).HasColumnName("CompanyName");
            deliveryInfo.Property(x => x.TaxCode).HasColumnName("TaxCode");
        });

        builder.OwnsMany(x => x.LineItems, lineItem =>
        {
            lineItem.ToTable("StockOutItems");

            lineItem.HasKey(x => x.Id);

            lineItem.Property(x => x.Id)
                .HasConversion(x => x.Value, x => new StockOutItemId(x))
                .ValueGeneratedNever();

            lineItem.Property(x => x.ProductId)
                .HasConversion(x => x.Value, x => new ProductId(x));

            lineItem.Property(x => x.ProductCode)
                .HasConversion(x => x.Value, x => Code.From(x));

            lineItem.Property(x => x.Quantity)
                .HasConversion(x => x.Value, x => new Quantity(x));
        });
    }
}