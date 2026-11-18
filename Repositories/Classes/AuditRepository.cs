using SwiftDrop.Models.Entities;
using SwiftDrop.Repositories.Interfaces;

namespace SwiftDrop.Repositories.Classes
{
    public class AuditRepository : BaseRepository<AuditLog>, IAuditRepository
    {
        public AuditRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
