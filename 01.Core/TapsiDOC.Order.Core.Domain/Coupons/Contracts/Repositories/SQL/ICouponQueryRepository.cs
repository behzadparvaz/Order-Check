using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Coupons.Entities;

namespace TapsiDOC.Order.Core.Domain.Coupons.Contracts.Repositories.SQL
{
    public interface ICouponQueryRepository
    {
        Task<Coupon> GetCouponByCode(string code, CancellationToken cancellationToken = default);
        Task<List<CouponUse>> GetCouponUses(long userId, long couponId, CancellationToken cancellationToken = default);
    }
}
