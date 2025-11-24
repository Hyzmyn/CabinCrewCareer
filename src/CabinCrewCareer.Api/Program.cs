using CabinCrewCareer.Api;
using CabinCrewCareer.Api.Startups;
using CabinCrewCareer.Application;
using CabinCrewCareer.Infrastructure;
using OfficeOpenXml;

// Set EPPlus License for version 8+ (NonCommercial use)
ExcelPackage.License.SetNonCommercialOrganization("CabinCrewCareer");

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services
        .AddPresentation()
        .AddInfrastructure(builder.Configuration)
        .AddApplication();
    builder.Services.AddSignalR();
}

var app = builder.Build();
{
    app.ConfigureSwagger();

    // CORS must be before Authentication/Authorization
    app.UseCors("AllowOrigin");

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    // Register SignalR endpoints
    app.MapHub<CabinCrewCareer.Infrastructure.Hubs.NotificationHub>("/notificationHub");

    app.Run();
}