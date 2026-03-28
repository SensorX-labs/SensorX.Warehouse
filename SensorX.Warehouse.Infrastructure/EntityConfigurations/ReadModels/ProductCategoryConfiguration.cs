using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Warehouse.Application.Queries.ReadModels;

namespace SensorX.Warehouse.Infrastructure.EntityConfigurations.ReadModels;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasOne<ProductCategory>()
               .WithMany()
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}