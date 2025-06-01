using System;

namespace TapsiDOC.Order.Core.Domain.Coupons.Exceptions
{
    public class NotFoundException : BaseCouponException
    {
        private new const string _Message = "اطلاعات یافت نشد.";

        public NotFoundException() : base(_Message)
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}
