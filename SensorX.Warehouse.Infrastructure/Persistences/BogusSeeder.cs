using Bogus;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace SensorX.Warehouse.Infrastructure.Persistences;

public static class BogusSeeder
{
    public static async Task SeedData(AppDbContext context)
    {
        if (await context.Set<InventoryItem>().AnyAsync())
        {
            return;
        }

        var faker = new Faker("vi");

        var inventoryItemFaker = new Faker<InventoryItem>("vi")
            .CustomInstantiator(f =>
            {
                var id = InventoryItemId.New();
                var productId = ProductId.New();
                var location = new WarehouseItemLocation(
                    "WH-01",
                    "ZONE-A",
                    f.Random.Number(1, 10).ToString(),
                    f.Random.Number(1, 5).ToString()
                );
                var physicalQty = new Quantity(f.Random.Number(100, 1000));
                var allocatedQty = new Quantity(f.Random.Number(0, 50));

                return new InventoryItem(
                    id,
                    productId,
                    location,
                    physicalQty,
                    allocatedQty
                );
            });

        var items = inventoryItemFaker.Generate(40);
        await context.Set<InventoryItem>().AddRangeAsync(items);
        await context.SaveChangesAsync();
    }
}
