using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;
using System.Globalization;

namespace SensorX.Warehouse.Application.Queries.Analytics.GetWarehouseDashboardStats;

public class GetWarehouseDashboardStatsHandler(
    IQueryBuilder<InventoryItem> _inventoryItemBuilder,
    IQueryBuilder<StockIn> _stockInBuilder,
    IQueryBuilder<StockOut> _stockOutBuilder,
    IQueryExecutor _queryExecutor,
    IDataServiceClient _dataServiceClient
) : IRequestHandler<GetWarehouseDashboardStatsQuery, Result<WarehouseReportStatsResponse>>
{
    public async Task<Result<WarehouseReportStatsResponse>> Handle(GetWarehouseDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // 1. Determine Time Range
        var now = DateTimeOffset.UtcNow;
        var startOfPeriod = GetStartOfPeriod(request.TimeRange, now);
        var startOfPreviousPeriod = GetStartOfPreviousPeriod(request.TimeRange, now, startOfPeriod);

        // 2. Fetch Warehouse Data
        var inventoryItems = await _queryExecutor.ToListAsync(_inventoryItemBuilder.QueryAsNoTracking, cancellationToken);
        var stockIns = await _queryExecutor.ToListAsync(_stockInBuilder.QueryAsNoTracking.Where(x => x.ReceivedDate >= startOfPreviousPeriod), cancellationToken);
        var stockOuts = await _queryExecutor.ToListAsync(_stockOutBuilder.QueryAsNoTracking.Where(x => x.CreatedAt >= startOfPreviousPeriod), cancellationToken);

        // Calculate quantities
        var currentStockIns = stockIns.Where(x => x.ReceivedDate >= startOfPeriod).ToList();
        var previousStockIns = stockIns.Where(x => x.ReceivedDate >= startOfPreviousPeriod && x.ReceivedDate < startOfPeriod).ToList();

        var currentStockOuts = stockOuts.Where(x => x.CreatedAt >= startOfPeriod).ToList();
        var previousStockOuts = stockOuts.Where(x => x.CreatedAt >= startOfPreviousPeriod && x.CreatedAt < startOfPeriod).ToList();

        var inboundThisPeriod = currentStockIns.SelectMany(x => x.LineItems).Sum(x => x.Quantity.Value);
        var inboundPreviousPeriod = previousStockIns.SelectMany(x => x.LineItems).Sum(x => x.Quantity.Value);
        var inboundGrowthPercent = CalculateGrowth(inboundThisPeriod, inboundPreviousPeriod);

        var outboundThisPeriod = currentStockOuts.SelectMany(x => x.LineItems).Sum(x => x.Quantity.Value);
        var outboundPreviousPeriod = previousStockOuts.SelectMany(x => x.LineItems).Sum(x => x.Quantity.Value);
        var outboundGrowthPercent = CalculateGrowth(outboundThisPeriod, outboundPreviousPeriod);

        var totalInventory = inventoryItems.Sum(x => x.PhysicalQuantity.Value);

        // 3. Fetch Product Context from Data Service
        var productContexts = await _dataServiceClient.GetProductPricingContextAsync(cancellationToken);
        var productContextDict = productContexts.ToDictionary(x => x.ProductId, x => x);

        // 4. Calculate Inventory Value and Category Distribution
        decimal totalInventoryValue = 0;
        var categoryMap = new Dictionary<string, CategoryTableData>();

        foreach (var item in inventoryItems)
        {
            var productContext = productContextDict.GetValueOrDefault(item.ProductId.Value);
            var categoryName = productContext?.CategoryName ?? "Khác";
            var price = productContext?.CurrentPrice ?? 0;
            var value = item.PhysicalQuantity.Value * price;

            totalInventoryValue += value;

            if (!categoryMap.TryGetValue(categoryName, out var categoryData))
            {
                categoryData = new CategoryTableData
                {
                    CategoryName = categoryName,
                    TotalItems = 0,
                    InStock = 0,
                    Imported = 0,
                    Exported = 0,
                    Value = 0
                };
                categoryMap[categoryName] = categoryData;
            }

            categoryData.InStock += item.PhysicalQuantity.Value;
            categoryData.Value += value;
        }

        // Count unique products per category
        var productCategoryMap = productContexts
            .GroupBy(x => x.CategoryName)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ProductId).Distinct().Count());

        foreach (var cat in categoryMap.Values)
        {
            cat.TotalItems = productCategoryMap.GetValueOrDefault(cat.CategoryName, 0);
        }

        // Add Inbound/Outbound to category map
        foreach (var stockIn in currentStockIns)
        {
            foreach (var lineItem in stockIn.LineItems)
            {
                var productContext = productContextDict.GetValueOrDefault(lineItem.ProductId.Value);
                var categoryName = productContext?.CategoryName ?? "Khác";
                if (categoryMap.TryGetValue(categoryName, out var categoryData))
                {
                    categoryData.Imported += lineItem.Quantity.Value;
                }
            }
        }

        foreach (var stockOut in currentStockOuts)
        {
            foreach (var lineItem in stockOut.LineItems)
            {
                var productContext = productContextDict.GetValueOrDefault(lineItem.ProductId.Value);
                var categoryName = productContext?.CategoryName ?? "Khác";
                if (categoryMap.TryGetValue(categoryName, out var categoryData))
                {
                    categoryData.Exported += lineItem.Quantity.Value;
                }
            }
        }

        var colors = new[] { "#3b82f6", "#10b981", "#f59e0b", "#6366f1", "#8b5cf6", "#ec4899", "#14b8a6", "#f43f5e" };
        var categoryDistribution = categoryMap.Values
            .OrderByDescending(x => x.Value)
            .Select((x, index) => new CategoryDistributionData
            {
                Name = x.CategoryName,
                Value = totalInventoryValue > 0 ? (x.Value / totalInventoryValue) * 100 : 0,
                Color = colors[index % colors.Length]
            })
            .Where(x => x.Value > 0)
            .ToList();

        // 5. Build Chart Data
        var chartData = BuildChartData(request.TimeRange, startOfPeriod, now, currentStockIns, currentStockOuts);

        var response = new WarehouseReportStatsResponse
        {
            TotalInventory = totalInventory,
            InboundThisPeriod = inboundThisPeriod,
            InboundGrowthPercent = inboundGrowthPercent,
            OutboundThisPeriod = outboundThisPeriod,
            OutboundGrowthPercent = outboundGrowthPercent,
            TotalInventoryValue = totalInventoryValue,
            CategoryTableData = [.. categoryMap.Values.OrderByDescending(x => x.Value)],
            CategoryDistribution = categoryDistribution,
            InboundOutboundChart = chartData
        };

        return Result<WarehouseReportStatsResponse>.Success(response);
    }

    private static decimal CalculateGrowth(int current, int previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round(((decimal)current - previous) / previous * 100, 2);
    }

    private static DateTimeOffset GetStartOfPeriod(string timeRange, DateTimeOffset now)
    {
        return timeRange.ToLower() switch
        {
            "today" => now.Date,
            "week" => now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday),
            "month" => new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset),
            "year" => new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset),
            _ => new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset),
        };
    }

    private static DateTimeOffset GetStartOfPreviousPeriod(string timeRange, DateTimeOffset now, DateTimeOffset startOfPeriod)
    {
        return timeRange.ToLower() switch
        {
            "today" => startOfPeriod.AddDays(-1),
            "week" => startOfPeriod.AddDays(-7),
            "month" => startOfPeriod.AddMonths(-1),
            "year" => startOfPeriod.AddYears(-1),
            _ => startOfPeriod.AddMonths(-1),
        };
    }

    private static List<InboundOutboundChartData> BuildChartData(string timeRange, DateTimeOffset start, DateTimeOffset end, List<StockIn> stockIns, List<StockOut> stockOuts)
    {
        var chartData = new List<InboundOutboundChartData>();

        if (timeRange == "today")
        {
            for (int i = 0; i <= 24; i += 2)
            {
                var periodStart = start.AddHours(i);
                var periodEnd = start.AddHours(i + 2);
                
                if (periodStart > end) break;

                chartData.Add(new InboundOutboundChartData
                {
                    Period = periodStart.ToString("HH:00"),
                    Inbound = stockIns.Where(x => x.ReceivedDate >= periodStart && x.ReceivedDate < periodEnd).SelectMany(x => x.LineItems).Sum(x => x.Quantity.Value),
                    Outbound = stockOuts.Where(x => x.CreatedAt >= periodStart && x.CreatedAt < periodEnd).SelectMany(x => x.LineItems).Sum(x => x.Quantity.Value)
                });
            }
        }
        else if (timeRange == "week")
        {
            var cultureInfo = new CultureInfo("vi-VN");
            for (int i = 0; i < 7; i++)
            {
                var periodStart = start.AddDays(i);
                var periodEnd = start.AddDays(i + 1);
                
                if (periodStart > end.Date.AddDays(1)) break;

                var dayName = periodStart.DayOfWeek == DayOfWeek.Sunday ? "CN" : "T" + ((int)periodStart.DayOfWeek + 1);
                
                chartData.Add(new InboundOutboundChartData
                {
                    Period = dayName,
                    Inbound = stockIns.Where(x => x.ReceivedDate >= periodStart && x.ReceivedDate < periodEnd).SelectMany(x => x.LineItems).Sum(x => x.Quantity.Value),
                    Outbound = stockOuts.Where(x => x.CreatedAt >= periodStart && x.CreatedAt < periodEnd).SelectMany(x => x.LineItems).Sum(x => x.Quantity.Value)
                });
            }
        }
        else if (timeRange == "month")
        {
            for (int i = 0; i < 4; i++)
            {
                var periodStart = start.AddDays(i * 7);
                var periodEnd = i == 3 ? start.AddMonths(1) : start.AddDays((i + 1) * 7);
                
                if (periodStart > end) break;

                chartData.Add(new InboundOutboundChartData
                {
                    Period = $"Tuần {i + 1}",
                    Inbound = stockIns.Where(x => x.ReceivedDate >= periodStart && x.ReceivedDate < periodEnd).SelectMany(x => x.LineItems).Sum(x => x.Quantity.Value),
                    Outbound = stockOuts.Where(x => x.CreatedAt >= periodStart && x.CreatedAt < periodEnd).SelectMany(x => x.LineItems).Sum(x => x.Quantity.Value)
                });
            }
        }
        else // year
        {
            for (int i = 0; i < 12; i++)
            {
                var periodStart = start.AddMonths(i);
                var periodEnd = start.AddMonths(i + 1);
                
                if (periodStart > end) break;

                chartData.Add(new InboundOutboundChartData
                {
                    Period = $"Tháng {i + 1}",
                    Inbound = stockIns.Where(x => x.ReceivedDate >= periodStart && x.ReceivedDate < periodEnd).SelectMany(x => x.LineItems).Sum(x => x.Quantity.Value),
                    Outbound = stockOuts.Where(x => x.CreatedAt >= periodStart && x.CreatedAt < periodEnd).SelectMany(x => x.LineItems).Sum(x => x.Quantity.Value)
                });
            }
        }

        return chartData;
    }
}
