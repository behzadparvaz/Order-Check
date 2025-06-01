using System;

namespace TapsiDOC.Order.Core.Domain.Coupons.Exceptions
{
    public class BadRequestException : BaseCouponException
    {
        private new const string _Message = "درخواست اشتباه.";

        public BadRequestException() : base(_Message)
        {
        }

        public BadRequestException(string message) : base(message)
        {
        }

        public BadRequestException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}
