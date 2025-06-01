using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using TapsiDOC.Order.Core.Domain.Drnext.Entities;
using TapsiDOC.Order.Infra.Data.Sql.Commands.Drnext.Persistence.SQL;

namespace TapsiDOC.Order.Core.ApplicationService.Drnext.Commands.CreateOrder
{
    internal class CreateOrderHandler : IRequestHandler<CreateOrderCommand, CreateOrderDto>
    {
        private readonly DrnextCommandDataContext _drnextCommandDataContext;
        private readonly IConfiguration _configuration;
        public CreateOrderHandler(DrnextCommandDataContext drnextCommandDataContext, IConfiguration configuration)
        {
            _drnextCommandDataContext = drnextCommandDataContext;
            _configuration = configuration;
        }

        public async Task<CreateOrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var rxRequest = Request.Create(
                request.firstName,
                request.lastName,
                request.nationalCode,
                request.mobile,
                request.patientInsurance.ToString(),
                request.visitedAt);
            _drnextCommandDataContext.Requests.Add(rxRequest);

            foreach (var patientDrug in request.patientDrugs)
            {
                var prescription = Prescription.Create(
                    patientDrug.amount,
                    patientDrug.direction,
                    patientDrug.description,
                    patientDrug.repeat,
                    patientDrug.dateOfDo,
                    JsonConvert.SerializeObject(patientDrug.unit));

                await _drnextCommandDataContext.Prescriptions.AddAsync(prescription);

                var drug = patientDrug.drug;
                var prescriptionDrugs = PrescriptionDrugs.Create(
                    prescription.Id,
                    drug.erxCode,
                    drug.genericCode,
                    drug.genericName,
                    drug.brandName,
                    drug.dosageForm,
                    drug.moleculeName,
                    drug.dose,
                    drug.brand,
                    drug.description);

                _drnextCommandDataContext.PrescriptionDrugs.Add(prescriptionDrugs);

            }
            await _drnextCommandDataContext.SaveChangesAsync(cancellationToken);
            var token = await GetToken(request.mobile);
            await AddToBasket(request, token);
            var redirectUrl = _configuration["DrnextRedirectUrl"] ?? throw new Exception("REDIRECT URL NOT FOUND");
            return new CreateOrderDto()
            {
                RedirectUrl = string.Format(redirectUrl, token)
            };
        }
        private async Task<string> GetToken(string phoneNumber)
        {
            var url = _configuration["LoginWithSso"]
               ?? throw new Exception("LoginWithSso URL NOT FOUND");
            var client = new RestClient();
            var request = new RestRequest(url, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-ServiceId", "TAPSIDR-877A543D88E242F2BDB3D95E05185B03");
            var body = new
            {
                phoneNumber
            };
            request.AddStringBody(JsonConvert.SerializeObject(body), DataFormat.Json);
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful == false || response.IsSuccessStatusCode == false || string.IsNullOrEmpty(response.Content))
                throw new Exception("خطا در دریافت توکن.");

            var token = JObject.Parse(response.Content)["token"];
            return token.ToString();
        }
        private async Task AddToBasket(CreateOrderCommand req, string token)
        {
            var url = _configuration["Basket:AddToCart"]
                 ?? throw new Exception("ADD TO CART URL NOT FOUND");
            var client = new RestClient();
            var request = new RestRequest(url, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            var body = new
            {
                orderType = "RX",
                refrenceNumber = req.prescriptionTrackingCode,
                nationalCode = req.nationalCode,
                phoneNumber = req.mobile,
                insuranceTypeId = req.patientInsurance,
            };
            request.AddStringBody(JsonConvert.SerializeObject(body), DataFormat.Json);
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful == false || response.IsSuccessStatusCode == false)
                throw new Exception("عملیات با خطا مواجد شد لطفا اطلاعات خود را بررسی کنید.");
        }
    }
}
