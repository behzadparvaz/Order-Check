using TapsiDOC.Order.Core.Domain.Coupons.Contracts.Repositories.SQL;
using TapsiDOC.Order.Core.Domain.Coupons.Entities;
using TapsiDOC.Order.Infra.Data.Sql.Commands.Coupons.Persistence.SQL;

namespace TapsiDOC.Order.Infra.Data.Sql.Commands.Coupons.Repositories.SQL
{
    public class CouponCommandRepository : ICouponCommandRepository
    {
        private readonly CommandDataContext _dbContext;

        public CouponCommandRepository(CommandDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CouponUse> AddCouponUse(CouponUse couponUse, CancellationToken cancellationToken = default)
        {
            _dbContext.CouponUses.Add(couponUse);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return couponUse;
        }
        public async Task<bool> UpdateRangeCouponUses(List<CouponUse> couponUses, CancellationToken cancellationToken = default)
        {
            _dbContext.UpdateRange(couponUses);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
