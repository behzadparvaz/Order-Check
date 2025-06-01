using Microsoft.EntityFrameworkCore;
using TapsiDOC.Order.Core.Domain.Coupons.Contracts.Repositories.SQL;
using TapsiDOC.Order.Core.Domain.Coupons.Entities;
using TapsiDOC.Order.Infra.Data.Sql.Queries.Coupons.Persistence.SQL;

namespace TapsiDOC.Order.Infra.Data.Sql.Queries.Coupons.Repositories.SQL
{
    public class CouponQueryRepository : ICouponQueryRepository
    {
        private readonly QueryDataContext _dbContext;

        public CouponQueryRepository(QueryDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Coupon?> GetCouponByCode(string code, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Coupons.FirstOrDefaultAsync(z => z.IsDeleted == false && z.Code == code, cancellationToken);
        }
        public async Task<List<CouponUse>> GetCouponUses(long userId, long couponId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.CouponUses.Where(z => z.IsDeleted == false && z.UserId == userId && z.CouponId == couponId)
                .ToListAsync(cancellationToken);
        }
       
    }
}

