using Ardalis.Specification;

namespace SensorX.Warehouse.Domain.AggregatesModel.UserAggregate.Specifications;

public class UserByUsernameSpec : Specification<User>
{
    public UserByUsernameSpec(string username)
    {
        Query.Where(u => u.Username == username);
    }
}
