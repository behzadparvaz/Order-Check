using System;

namespace TapsiDOC.Order.Core.Domain.Coupons.Exceptions
{

    public abstract class BaseCouponException : Exception
    {
        protected const string _Message = "خطای رخ داده است.";
        public BaseCouponException() : base(_Message)
        {

        }
        public BaseCouponException(string message) : base(message)
        {

        }
        public BaseCouponException(string message, Exception exception) : base(message, exception)
        {

        }
    }
}
