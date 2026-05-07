using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Infrastructure.EntityConfigurations;

public class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("StockAdjustments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new StockAdjustmentId(x))
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => Code.From(x));

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.RejectReason)
            .HasMaxLength(500);

        builder.Property(x => x.WarehouseId)
            .HasConversion(x => x.Value, x => new WarehouseId(x));

        builder.OwnsMany(x => x.Items, item =>
        {
            item.ToTable("StockAdjustmentItems");

            item.HasKey(x => x.Id);

            item.Property(x => x.Id)
                .HasConversion(x => x.Value, x => new StockAdjustmentItemId(x))
                .ValueGeneratedNever();

            item.Property(x => x.ProductId)
                .HasConversion(x => x.Value, x => new ProductId(x));

            item.Property(x => x.ProductCode)
                .HasConversion(x => x.Value, x => Code.From(x));

            item.Property(x => x.Unit)
                .HasMaxLength(50);

            item.Property(x => x.AdjustedQuantity)
                .HasColumnName("AdjustedQuantity");

            item.Property(x => x.Note)
                .HasMaxLength(500);
        });
    }
}