using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interfaces;

public interface IApplicationUserRepository : IRepository<ApplicationUser>
{
    //nic tady nepotřebujeme
    //void Update(Amenity entity);
}