using Microsoft.EntityFrameworkCore;
using TapsiDOC.Order.Core.Domain.Drnext.Entities;

namespace TapsiDOC.Order.Infra.Data.Sql.Commands.Drnext.Persistence.SQL
{
    public class DrnextCommandDataContext : DbContext
    {
        public DbSet<PrescriptionDrugs> PrescriptionDrugs { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DrnextCommandDataContext(DbContextOptions<DrnextCommandDataContext> opt) : base(opt)
        {

        }
    }
}
