using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Coupons.Entities;

namespace TapsiDOC.Order.Core.Domain.Coupons.Contracts.Repositories.SQL
{
    public interface ICouponCommandRepository
    {
        Task<CouponUse> AddCouponUse(CouponUse couponUse, CancellationToken cancellationToken = default);
        Task<bool> UpdateRangeCouponUses(List<CouponUse> couponUses, CancellationToken cancellationToken = default);
    }
}
