using MediatR;
using Microsoft.AspNetCore.Mvc;
using OKEService.EndPoints.Web.Controllers;
using TapsiDOC.Order.Core.ApplicationService.Drnext.Commands.CreateOrder;

namespace TapsiDOC.Order.EndPoints.Api.V1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DrnextController : BaseController
    {
        private readonly IMediator _mediator;

        public DrnextController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<CreateOrderDto>> CreateOrder(CreateOrderCommand command)
        {
            try
            {
                if (Request.Headers["X-ServiceId"] != "TAPSIDR-877A5423D88E141F2BDB3D95E05185B04")
                    return Unauthorized();

                var res = await _mediator.Send(command);
                return Ok(res);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
