using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Infrastructure.EntityConfigurations;

public class StockInConfiguration : IEntityTypeConfiguration<StockIn>
{
    public void Configure(EntityTypeBuilder<StockIn> builder)
    {
        builder.ToTable("StockIns");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new StockInId(x))
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => Code.From(x));

        builder.Property(x => x.TransferOrderId)
            .HasConversion(x => x!.Value, x => new TransferOrderId(x))
            .IsRequired(false);

        builder.Property(x => x.TransferOrderCode)
            .HasConversion(x => x!.Value, x => Code.From(x))
            .IsRequired(false);

        builder.Property(x => x.WarehouseId)
            .HasConversion(x => x.Value, x => new WarehouseId(x));

        builder.OwnsMany(x => x.LineItems, lineItem =>
        {
            lineItem.ToTable("StockInItems");

            lineItem.HasKey(x => x.Id);

            lineItem.Property(x => x.Id)
                .HasConversion(x => x.Value, x => new StockInItemId(x))
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