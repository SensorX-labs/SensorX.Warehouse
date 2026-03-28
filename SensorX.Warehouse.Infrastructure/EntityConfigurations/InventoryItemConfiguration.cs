using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Infrastructure.EntityConfigurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new InventoryItemId(x))
            .ValueGeneratedNever();

        builder.Property(x => x.ProductId)
            .HasConversion(x => x.Value, x => new ProductId(x));

        builder.Property(x => x.PhysicalQuantity)
            .HasConversion(x => x.Value, x => new Quantity(x));

        builder.Property(x => x.AllocatedQuantity)
            .HasConversion(x => x.Value, x => new Quantity(x));

        builder.OwnsOne(x => x.WarehouseItemLocation, location =>
        {
            location.Property(l => l.WarehouseId).HasConversion(x => x.Value, x => new WarehouseId(x));
            location.Property(l => l.WarehouseName).HasColumnName("WarehouseName");
            location.Property(l => l.BrandZone).HasColumnName("BrandZone");
            location.Property(l => l.RackCode).HasColumnName("RackCode");
        });

    }
}