using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TapsiDOC.Order.Core.ApplicationService.Coupons.Commands.DeleteCoupon;
using TapsiDOC.Order.Core.ApplicationService.Coupons.Commands.UseCoupon;

namespace TapsiDOC.Order.EndPoints.Api.V1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class CouponController : Controller
    {
        private readonly IMediator _mediator;
        public CouponController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> UseCoupon(UseCouponCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                command.UserId = GetUserName();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(command, cancellationToken);
            return Json(result);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteCoupon(DeleteCouponCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                command.UserId = GetUserName();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(command, cancellationToken);
            return Json(result);
        }
        private long GetUserName()
        {
            var userId = User.Claims.FirstOrDefault(z => z.Type == ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException();
            return long.Parse(userId);

        }
    }


}
