using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Domain.AggregatesModel.UserAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.UserAggregate.Specifications;
using SensorX.Warehouse.Domain.Common.Utilities;
using SensorX.Warehouse.Domain.SeedWork;
using MediatR;

namespace SensorX.Warehouse.Application.Commands.Auth.Login;

public class LoginCommandHandler(IRepository<User> userRepository) : IRequestHandler<LoginCommand, string>
{
    private readonly IRepository<User> _userRepository = userRepository;

    public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var spec = new UserByUsernameSpec(request.Username);
        var user = await _userRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (user is null || user.PasswordHash != HashHelper.HashToken(request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // In a real scenario, generate a JWT token here.
        // For this sample, we return a simulated token.
        return $"Simulated-Token-For-{user.Id}";
    }
}
