using CabinCrewCareer.Api;
using CabinCrewCareer.Api.Startups;
using CabinCrewCareer.Application;
using CabinCrewCareer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services
        .AddPresentation()
        .AddInfrastructure(builder.Configuration)
        .AddApplication();
}

var app = builder.Build();
{
    app.ConfigureSwagger();

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseCors("AllowOrigin");

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}