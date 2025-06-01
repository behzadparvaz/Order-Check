using OKEService.Core.ApplicationServices.Commands;
using OKEService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.DoPayment;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using Aggreagte = TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CreateDraft
{
    public class CreateDraftCommandHandler : CommandHandler<CreateDraftCommand, string>
    {
        private readonly IOrderCommandRepository commandRepository;
        private readonly IOrderQueryRepository queryRepository;

        public CreateDraftCommandHandler(OKEServiceServices okeserviceServices, IOrderCommandRepository commandRepository, IOrderQueryRepository queryRepository) : base(okeserviceServices)
        {
            this.commandRepository = commandRepository;
            this.queryRepository = queryRepository;
        }

        public async override Task<CommandResult<string>> Handle(CreateDraftCommand request)
        {
            //Aggreagte.Order order = new();
            //var vendors = order.SetVendor(request.VendorSelects);
            //order = order.CreateRX(request.ReferenceNumber,request.InsuranceTypeId, request.SupplementaryInsuranceTypeId, "", "", request.DeliveryDate ,OrderDetailType.RX , 
            //                              request.IsSpecialPatient, PaymentType.Online, 
            //                              request.PhoneNumber, request.NationalCode,request.Latitude, request.Longitude, request.ValueAddress, request.TitleAddress, 
            //                              request.HouseNumber, 0, 0, request.CustomerName,request.DoctorName , request.Comment ,vendors, request.HomeUnit);
            //await commandRepository.CreateOrder(order);
            //return Ok(order.OrderCode);
            throw new NotImplementedException();
        }
    }
}
