using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.UserAggregate;

public class User(string username, string email, string passwordHash) : AuditEntity, IAggregateRoot
{
    public string Username { get; private set; } = username;
    public string Email { get; private set; } = email;
    public string PasswordHash { get; private set; } = passwordHash;

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }
}
