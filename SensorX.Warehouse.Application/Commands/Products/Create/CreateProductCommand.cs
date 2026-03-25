using MediatR;

namespace SensorX.Warehouse.Application.Commands.Products.Create;

public record CreateProductCommand(string Name, string Description, decimal Price) : IRequest<long>;
