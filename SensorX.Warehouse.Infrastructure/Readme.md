# Để tạo 1 migartion mới

dotnet ef migrations add <Tên migration> --context AppDbContext -p ./SensorX.Warehouse.Infrastructure/SensorX.Warehouse.Infrastructure.csproj -s ./SensorX.Warehouse.WebApi/SensorX.Warehouse.WebApi.csproj --output-dir Persistences/Migrations

# Để gỡ migration gần nhất

dotnet ef migrations remove --context AppDbContext -p ./SensorX.Warehouse.Infrastructure/SensorX.Warehouse.Infrastructure.csproj -s ./SensorX.Warehouse.WebApi/SensorX.Warehouse.WebApi.csproj

# Để xem danh sách migration

dotnet ef migrations list --context AppDbContext -p ./SensorX.Warehouse.Infrastructure/SensorX.Warehouse.Infrastructure.csproj -s ./SensorX.Warehouse.WebApi/SensorX.Warehouse.WebApi.csproj

# Để cập nhật migration lên Database 

dotnet ef database update --context AppDbContext -p ./SensorX.Warehouse.Infrastructure/SensorX.Warehouse.Infrastructure.csproj -s ./SensorX.Warehouse.WebApi/SensorX.Warehouse.WebApi.csproj

# Để cập nhật migration cụ thể lên Database 

dotnet ef database update <Tên migration> --context AppDbContext -p ./SensorX.Warehouse.Infrastructure/SensorX.Warehouse.Infrastructure.csproj -s ./SensorX.Warehouse.WebApi/SensorX.Warehouse.WebApi.csproj

# Xóa database
dotnet ef database drop --context AppDbContext -p ./SensorX.Warehouse.Infrastructure/SensorX.Warehouse.Infrastructure.csproj -s ./SensorX.Warehouse.WebApi/SensorX.Warehouse.WebApi.csproj