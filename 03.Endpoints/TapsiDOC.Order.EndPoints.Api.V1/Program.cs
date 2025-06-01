using OKEService.EndPoints.Web;
using TapsiDOC.Order.EndPoints.Api.V1;

if (WebApplication.CreateBuilder(args).Environment.EnvironmentName == "Development")
    new OKEServiceProgram().Main(args, typeof(Services), $"appsettings.Development.json", "appsettings.okeservice.json", "appsettings.Development.serilog.json");
else
    new OKEServiceProgram().Main(args, typeof(Services), $"appsettings.json", "appsettings.okeservice.json", "appsettings.serilog.json");