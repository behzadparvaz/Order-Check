using Microsoft.EntityFrameworkCore;
using TapsiDOC.Order.Core.Domain.Coupons.Entities;

namespace TapsiDOC.Order.Infra.Data.Sql.Commands.Coupons.Persistence.SQL
{
    public class CommandDataContext : DbContext
    {
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponUse> CouponUses { get; set; }

        public CommandDataContext(DbContextOptions<CommandDataContext> opt) : base(opt)
        {

        }
    }
}
