using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Warehouse.Application.Queries.ReadModels;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Infrastructure.EntityConfigurations.ReadModels;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ManufactureName).HasMaxLength(100).IsRequired();

        builder.HasOne(x => x.Category)
               .WithMany()
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.OwnsMany(x => x.Attributes, a =>
        {
            a.ToTable("ProductAttributes");
            a.WithOwner().HasForeignKey("ProductId");
            a.HasKey(x => x.Id);
            a.Property(x => x.Id).ValueGeneratedNever();
        });

        builder.OwnsMany(x => x.Images, i =>
        {
            i.ToTable("ProductImages");
            i.WithOwner().HasForeignKey("ProductId");
            i.HasKey(x => x.Id);
            i.Property(x => x.Id).ValueGeneratedNever();
        });
    }
}