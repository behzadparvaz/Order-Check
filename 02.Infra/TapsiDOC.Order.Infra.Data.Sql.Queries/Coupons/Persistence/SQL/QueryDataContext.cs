using Microsoft.EntityFrameworkCore;
using TapsiDOC.Order.Core.Domain.Coupons.Entities;

namespace TapsiDOC.Order.Infra.Data.Sql.Queries.Coupons.Persistence.SQL
{
    public class QueryDataContext : DbContext
    {
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponUse> CouponUses { get; set; }

        public QueryDataContext(DbContextOptions<QueryDataContext> opt) : base(opt)
        {

        }
    }
}
