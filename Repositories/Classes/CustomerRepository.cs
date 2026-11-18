using SwiftDrop.Models.Entities;
using SwiftDrop.Repositories.Interfaces;

namespace SwiftDrop.Repositories.Classes
{
    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
