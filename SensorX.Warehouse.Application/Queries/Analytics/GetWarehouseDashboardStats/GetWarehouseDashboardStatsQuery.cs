using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Queries.Analytics.GetWarehouseDashboardStats;

public record GetWarehouseDashboardStatsQuery(string TimeRange = "month") : IRequest<Result<WarehouseReportStatsResponse>>;

public class WarehouseReportStatsResponse
{
    public int TotalInventory { get; set; }
    public int InboundThisPeriod { get; set; }
    public decimal InboundGrowthPercent { get; set; }
    public int OutboundThisPeriod { get; set; }
    public decimal OutboundGrowthPercent { get; set; }
    public decimal TotalInventoryValue { get; set; }

    public List<InboundOutboundChartData> InboundOutboundChart { get; set; } = [];
    public List<CategoryDistributionData> CategoryDistribution { get; set; } = [];
    public List<CategoryTableData> CategoryTableData { get; set; } = [];
}

public class InboundOutboundChartData
{
    public string Period { get; set; } = string.Empty;
    public int Inbound { get; set; }
    public int Outbound { get; set; }
}

public class CategoryDistributionData
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class CategoryTableData
{
    public string CategoryName { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public int InStock { get; set; }
    public int Imported { get; set; }
    public int Exported { get; set; }
    public decimal Value { get; set; }
}
