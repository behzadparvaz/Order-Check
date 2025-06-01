using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using OKEService.Core.Domain.Data;
using OKEService.EndPoints.Web.Controllers;
using System.Globalization;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.AcceptOrder;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.ActiveItemRequestOrder;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.AlternativeProduct;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.AssignmentOrder;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CancelOrder;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CancelOrderCallCenter;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CancelVendorOrder;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CreateDraft;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.DoPayment;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.InquiryPayment;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.NFCConnection;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderDraft;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderScheduled;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.PaymentOrder;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.RejectItemRequestOrder;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.UpdateDeliveryPriceRoyal;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.UpdateOrderStatus;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.VerifyPaymentOrder;
using TapsiDOC.Order.Core.ApplicationService.Orders.Queries.FindOrder;
using TapsiDOC.Order.Core.ApplicationService.Orders.Queries.GetOrderDraftByOrderCode;
using TapsiDOC.Order.Core.ApplicationService.Orders.Queries.GetOrdersHistory;
using TapsiDOC.Order.Core.ApplicationService.Orders.Queries.GetVendor;
using TapsiDOC.Order.Core.ApplicationService.Orders.Queries.LastOrderByUser;
using TapsiDOC.Order.Core.ApplicationService.Orders.Queries.OrderTenders;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.QueryContract;
using MyEntities = TapsiDOC.Order.Core.Domain.Orders.Entities;
using Microsoft.AspNetCore.Components.Forms;
using TapsiDOC.Order.Core.ApplicationService.Orders.Queries.FindPhoneNumber;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CreateReview;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.JobCancelCustomer;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderScheduledFoAllVendors;

namespace TapsiDOC.Order.EndPoints.Api.V1.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrderController : BaseController
    {        
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IMediator mediator ,
            IServiceProvider serviceProvider,
            ILogger<OrderController> logger)
        { 
            _mediator = mediator;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [HttpPost("[Controller]/CreateOrderInsurance")]
        public async Task<IActionResult> Post([FromBody] CreateDraftCommand command)
        {
            try
            {
                command.PhoneNumber = GetUserName();
                return await Create<CreateDraftCommand, string>(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateOrderInsurance exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpPost("[Controller]/CreateOrderDraftMedicalAdvice")]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] ActiveItemRequestOrderCommand command)
        {
            try
            {

                var json = JsonConvert.SerializeObject(command,
                        new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                this._logger.LogInformation($"CreateOrderDraftMedicalAdvice Data :{json}");
                if (Request.Headers["X-ServiceId"] != "TAPSIDR-877A543D88E141F2BDB3D95E05185B04")
                {
                    return Unauthorized();
                }
                return await Create(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateOrderDraftMedicalAdvice exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpPost("[Controller]/RejectRequestOrderMedicalAdvice")]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] RejectItemRequestOrder command)
        {
            try
            {
                if (Request.Headers["X-ServiceId"] != "TAPSIDR-877A543D88E141F2BDB3D95E05185B04")
                {
                    return Unauthorized();
                }
                return await Create(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("RejectRequestOrderMedicalAdvice exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpPost("[Controller]/CreateOrderDraft")]
        public async Task<IActionResult> Post([FromBody] OrderDraftCommand command)
        {
            try
            {
                _logger?.LogInformation("CreateOrderDraft started. Input: {@command}", command);

                command.Token = this.Request.Headers[HeaderNames.Authorization];
                command.PhoneNumber = GetUserName();   

                //_taskQueue.QueueBackgroundWorkItem(async token =>
                //    {
                //        using (var scope = _serviceProvider.CreateScope())
                //        {
                //            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                //            await mediator.Send(command);
                //        }
                //    });
                var orderCode = await _mediator.Send(command);

                return Ok(orderCode);
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateOrderDraft exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpPost("[Controller]/AssignmentOrder")]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] AssignmentOrderCommand command)
        {
            if (Request.Headers["X-ServiceId"] != "TAPSIDR-877A543D88E141F2BDB3D95E05185B04")
            {
                return Unauthorized();
            }
            return await Create(command);
        }

        [AllowAnonymous]
        [HttpPost("[Controller]/DoPayment")]
        public async Task<IActionResult> DoPayment([FromBody] DoPaymentCommand command)
        {
            try
            {
                return await Create(command);
            }
            catch (Exception ex)
            { 
                _logger.LogError("DoPayment exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("[Controller]/NFC")]
        public async Task<IActionResult> NFC([FromBody] NFCConnectionCommand command)
        {
            try
            {
                return await Create(command);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpPost("[Controller]/PaymentOrder")]
        public async Task<IActionResult> FinishPayment([FromBody] PaymentOrderCommand command)
        {
            try
            {
                command.PhoneNumber = GetUserName();
                return await Create<PaymentOrderCommand, string>(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("PaymentOrder exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }


        [HttpPost("[Controller]/DeliveryPriceRoyal")]
        public async Task<IActionResult> FinishPayment([FromBody] UpdateDeliveryPriceRoyalCommand command)
        {
            try
            {
                return await Create(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("PaymentOrder exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpPost("[Controller]/VerifyPaymentOrder")]
        public async Task<IActionResult> Post([FromBody] VerifyPaymentOrderCommand command)
        {
            try
            {
                return await Create<VerifyPaymentOrderCommand, PaymentData>(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("VerifyPaymentOrder exception: {@ex}", ex);
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }



        [HttpPost("[Controller]/VerifyPayment")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentOrderCommand command)
        {
            try
            {
                if (Request.Headers["X-ServiceId"] != "TAPSIDR-877A543D88E141F2BDB3D95E05185B04")
                {
                    return Unauthorized();
                }
                return await Create<VerifyPaymentOrderCommand, PaymentData>(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("VerifyPaymentOrder exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }


        [HttpPost("[Controller]/InquiryPayment")]
        [AllowAnonymous]
        public async Task<IActionResult> InquiryPayment([FromBody] InquiryPaymentCommand command)
        {
            try
            {
                if (Request.Headers["X-ServiceId"] != "TAPSIDR-877A543D88E141F2BDB3D95E05185B04")
                {
                    return Unauthorized();
                }
                return await Create<InquiryPaymentCommand,bool>(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("VerifyPaymentOrder exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("[Controller]/AcceptOrder")]
        public async Task<IActionResult> FinishPayment([FromBody] AcceptOrderCommand command)
        {
            try
            {
                return await Create(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("AcceptOrder exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("[Controller]/CancelVendorOrder")]
        public async Task<IActionResult> CancelVendorOrder([FromBody] CancelVendorOrderCommand command)
        {
            try
            {
                return await Create(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("CancelVendorOrder exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpPost("[Controller]/CancelOrderCustomer")]
        public async Task<IActionResult> CancelOrder([FromBody] CancelOrderCommand command)
        {
            try
            {
                command.PhoneNumber = GetUserName();
                return await Create(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("CancelOrder exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpPost("[Controller]/CancelOrderCallCenter")]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] CancelOrderCallCenterCommand command)
        {
            try
            {
                if (Request.Headers["X-ServiceId"] != "TAPSIDR-877A543D88E141F2BDB3D95E05185B04")
                {
                    return Unauthorized();
                }
                return await Create(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("CancelOrderCallCenter exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpPost("[Controller]/AlternativeOrder")]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] AlternativeProductCommand command)
        {
            try
            {
                return await Create<AlternativeProductCommand, bool>(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("AlternativeOrder exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpPost("[controller]/[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusCommand command)
        {
            try
            {
                return await Create(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateOrderStatus exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpGet("[Controller]/GetOrdersHistory")]
        public async Task<IActionResult> GetOrdersHistory([FromHeader] GetOrdersHistoryQuery query)
        {
            try
            {
                query.PhoneNumber = GetUserName();
                return await Query<GetOrdersHistoryQuery, List<MyEntities.Order>>(query);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpGet("[Controller]/CurrentOrder")]
        public async Task<IActionResult> Get([FromHeader] LastOrderByUserQuery query)
        {
            try
            {
                query.PhoneNumber = GetUserName();
                return await Query<LastOrderByUserQuery, MyEntities.Order>(query);

            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpGet("[Controller]/GetInsurances")]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = InsuranceType.List().Skip(1).Take(10);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpGet("[Controller]/GetSupplementaryInsurances")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSupplementaryInsurances()
        {
            try
            {
                var result = SupplementaryInsuranceType.List().Skip(1).Take(100);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpGet("[Controller]/GetOrderStatues")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var result = OrderStatus.List();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpGet("[Controller]/GetVendors")]
        public async Task<IActionResult> Get([FromHeader] GetVendorQuery query)
        {
            try
            {
                return await Query<GetVendorQuery, List<VendorType>>(query);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpGet("[Controller]/{OrderCode}/CurrentState")]

        public async Task<IActionResult> Get([FromHeader] FindOrderQuery query)
        {
            try
            {
                query.PhoneNumber = GetUserName();
                return await Query<FindOrderQuery, MyEntities.Order>(query);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpGet("[Controller]/GetOrderDetails")]
        public async Task<IActionResult> Get([FromQuery] GetOrderDraftByOrderCodeQuery query)
        {
            try
            {
                query.PhoneNumber = GetUserName();
                return await Query<GetOrderDraftByOrderCodeQuery, MyEntities.Order>(query);
            }
            catch (UnauthorizedAccessException ex)
            {
                return new ForbidResult();               
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpGet("[Controller]/OrderTenders")]
        public async Task<IActionResult> Get([FromHeader] OrderTendersQuery query)
        {
            try
            {
                return await Query<OrderTendersQuery, PagedData<MyEntities.Order>>(query);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpGet("[Controller]/GetDeclineTypes")]
        public async Task<IActionResult> GetDeclineTypes()
        {
            try
            {
                return Ok(DeclineType.List().Where(a=>a.Id>=18 && a.Id<=27));
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }


        [HttpGet("[Controller]/GetDeclineTypesCallCenter")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDeclineTypesCallCenter()
        {
            try
            {
                if (Request.Headers["X-ServiceId"] != "TAPSIDR-877A543D88E141F2BDB3D95E05185B04")
                {
                    return Unauthorized();
                }
                return Ok(DeclineType.List().Where(a=>a.Id>=19 && a.Id<=31));
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpGet("[Controller]/DeliveryScheduleTime")]
        [AllowAnonymous]
        public IActionResult GetAvailableTimes()
        {
            PersianCalendar pc = new PersianCalendar();
            string[] persianWeekDays = { "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه" };

            DateTime now = DateTime.Now;
            int startDayOffset = now.Hour < 12 ? 0 : 1;
            int daysToShow = 3;

            List<object> availableTimes = new List<object>();

            for (int i = startDayOffset; i < startDayOffset + daysToShow; i++)
            {
                DateTime date = now.AddDays(i);
                int year = pc.GetYear(date);
                int month = pc.GetMonth(date);
                int day = pc.GetDayOfMonth(date);
                string weekDay = persianWeekDays[(int)date.DayOfWeek];

                string[] timeSlots = i == 0 && now.Hour < 12
                    ? new string[] { "ساعت 15 تا 21" }
                    : new string[] { "ساعت 9 تا 15", "ساعت 15 تا 21" };

                List<object> timeSlotsList = new List<object>();
                for (int j = 0; j < timeSlots.Length; j++)
                {
                    timeSlotsList.Add(new
                    {
                        id = j + 1,
                        timeRange = timeSlots[j]
                    });
                }
                availableTimes.Add(new
                {
                    date = $"{year}/{month:D2}/{day:D2}",
                    dayOfWeek = weekDay,
                    timeSlots = timeSlotsList
                });
            }

            return Ok(availableTimes);
        }


        //[HttpGet("[Controller]/DeliveryScheduleTime")]
        //[AllowAnonymous]
        //public IActionResult GetAvailableTimes()
        //{
        //    PersianCalendar pc = new PersianCalendar();
        //    string[] persianWeekDays = { "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه" };

        //    DateTime now = DateTime.Now;
        //    DateTime phase1Start = new DateTime(1404, 1, 9, pc); 
        //    DateTime phase1End = new DateTime(1404, 1, 15, pc); 

        //    List<object> availableTimes = new List<object>();

        //    if (now < phase1End)
        //    {
        //        for (int i = 16; i <= 18; i++)
        //        {
        //            DateTime date = new DateTime(1404, 1, i, pc);
        //            int year = pc.GetYear(date);
        //            int month = pc.GetMonth(date);
        //            int day = pc.GetDayOfMonth(date);
        //            string weekDay = persianWeekDays[(int)date.DayOfWeek];

        //            string[] timeSlots;
        //            timeSlots = new string[] { "ساعت 9 تا 15", "ساعت 15 تا 21" };
        //            List<object> timeSlotsList = timeSlots
        //                .Select((slot, index) => new { id = index + 1, timeRange = slot })
        //                .ToList<object>();

        //            availableTimes.Add(new
        //            {
        //                date = $"{year}/{month:D2}/{day:D2}",
        //                dayOfWeek = weekDay,
        //                timeSlots = timeSlotsList
        //            });
        //        }
        //    }
        //    else
        //    {
        //        int startDayOffset = now.Hour < 12 ? 0 : 1;
        //        int daysToShow = 3;

        //        for (int i = startDayOffset; i < startDayOffset + daysToShow; i++)
        //        {
        //            DateTime date = now.AddDays(i);
        //            int year = pc.GetYear(date);
        //            int month = pc.GetMonth(date);
        //            int day = pc.GetDayOfMonth(date);
        //            string weekDay = persianWeekDays[(int)date.DayOfWeek];

        //            string[] timeSlots = new string[] { "ساعت 9 تا 15", "ساعت 15 تا 21" };

        //            List<object> timeSlotsList = timeSlots
        //                .Select((slot, index) => new { id = index + 1, timeRange = slot })
        //                .ToList<object>();

        //            availableTimes.Add(new
        //            {
        //                date = $"{year}/{month:D2}/{day:D2}",
        //                dayOfWeek = weekDay,
        //                timeSlots = timeSlotsList
        //            });
        //        }
        //    }

        //    return Ok(availableTimes);
        //}




        [HttpGet("[Controller]/FindPhoneNumber")]
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromQuery] FindPhoneNumberQuery query)
        {
            try
            {
                if (Request.Headers["X-ServiceId"] != "TAPSIDR-5CABC4C0290C4C91A1E4BFD6E52AA544")
                {
                    return Unauthorized();
                }
                return await Query<FindPhoneNumberQuery, string>(query);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpGet("[Controller]/JobCancelOrder")]
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromHeader] JobCancelCustomerCommand query)
        {
            try
            {
                //if (Request.Headers["X-ServiceId"] != "TAPSIDR-5CABC4C0290C4C91A1E4BFD6E52AA544")
                //{
                //    return Unauthorized();
                //}
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        [HttpPost("[Controller]/Review")]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewCommand command)
        {
            try
            {
                _logger?.LogInformation("CreateOrderDraft started. Input: {@command}", command);

                command.UserPhoneNumber = GetUserName();
                //"09124934921";

                var orderCode = await _mediator.Send(command);

                return Ok(orderCode);
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateReview exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }


        [HttpPost("[Controller]/AssignOrderToAllVendors")]
        [AllowAnonymous]
        public async Task<IActionResult> AssignOrderToAllVendors([FromBody] OrderScheduledFoAllVendorsCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("AssignOrderToAllVendors exception: {@ex}", ex);

                return BadRequest(new
                {
                    errors = new { message = ex.Message },
                    status = 400
                });
            }
        }

        protected string GetUserName() => this.User.Claims?.First(i => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name".Equals(i.Type, StringComparison.OrdinalIgnoreCase))?.Value;
    }
}
