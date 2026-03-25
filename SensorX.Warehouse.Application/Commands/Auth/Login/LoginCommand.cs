using MediatR;

namespace SensorX.Warehouse.Application.Commands.Auth.Login;

public record LoginCommand(string Username, string Password) : IRequest<string>;
