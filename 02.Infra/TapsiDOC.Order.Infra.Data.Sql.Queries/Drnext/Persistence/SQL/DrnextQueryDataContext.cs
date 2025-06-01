using Microsoft.EntityFrameworkCore;
using TapsiDOC.Order.Core.Domain.Drnext.Entities;

namespace TapsiDOC.Order.Infra.Data.Sql.Queries.Drnext.Persistence.SQL
{
    public class DrnextQueryDataContext : DbContext
    {
        public DbSet<PrescriptionDrugs> PrescriptionDrugs { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DrnextQueryDataContext(DbContextOptions<DrnextQueryDataContext> opt) : base(opt)
        {

        }
    }
}
