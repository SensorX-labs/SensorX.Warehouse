FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SensorX.Warehouse.WebApi/SensorX.Warehouse.WebApi.csproj", "SensorX.Warehouse.WebApi/"]
COPY ["SensorX.Warehouse.Infrastructure/SensorX.Warehouse.Infrastructure.csproj", "SensorX.Warehouse.Infrastructure/"]
COPY ["SensorX.Warehouse.Application/SensorX.Warehouse.Application.csproj", "SensorX.Warehouse.Application/"]
COPY ["SensorX.Warehouse.Domain/SensorX.Warehouse.Domain.csproj", "SensorX.Warehouse.Domain/"]

RUN dotnet restore "SensorX.Warehouse.WebApi/SensorX.Warehouse.WebApi.csproj"

COPY . .
RUN dotnet publish "SensorX.Warehouse.WebApi/SensorX.Warehouse.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SensorX.Warehouse.WebApi.dll"]