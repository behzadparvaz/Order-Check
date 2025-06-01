using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CreateDraft
{
    public class CreateDraftCommandValidation:AbstractValidator<CreateDraftCommand>
    {
        public CreateDraftCommandValidation() 
        {
            RuleFor(a => a.ReferenceNumber).NotEmpty().NotNull().MinimumLength(3).WithMessage("order code non valid");            
            RuleFor(a => a.NationalCode).NotNull().NotEmpty().Length(10).WithMessage("enter national code carefuly");
        }
    }
}
