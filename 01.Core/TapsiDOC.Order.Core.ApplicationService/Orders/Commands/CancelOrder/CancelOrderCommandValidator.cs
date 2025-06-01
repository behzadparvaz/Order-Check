using FluentValidation;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CancelOrder
{
    public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
    {
        public CancelOrderCommandValidator()
        {
            RuleFor(p => p.OrderCode).NotEmpty();            
        }
    }
}
