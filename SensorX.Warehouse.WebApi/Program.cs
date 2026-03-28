using SensorX.Warehouse.Infrastructure.DI;
using SensorX.Warehouse.WebApi.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.Run();

