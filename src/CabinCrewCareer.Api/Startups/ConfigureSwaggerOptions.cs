using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CabinCrewCareer.Api.Startups
{
    public class ConfigureSwaggerOptions(
        IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider = provider;

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    new OpenApiInfo()
                    {
                        Version = description.ApiVersion.ToString(),
                        Title = $"Cabin Crew Career API {description.ApiVersion}",
                        Description = "A .NET Core Web API that allows you to work with recruitment and promotion process.",
                        Contact = new OpenApiContact
                        {
                            Name = "GFA25SE113",
                            Email = "example@email.com",
                            Url = new Uri("https://github.com/thanhnhan151/CabinCrewCareer"),
                        },
                        License = new OpenApiLicense
                        {
                            Name = "MIT License",
                            Url = new Uri("https://opensource.org/licenses/MIT")
                        },
                        TermsOfService = new Uri("https://example.com/terms"),
                    });
                options.AddServer(new OpenApiServer { Url = "https://localhost:7107", Description = "Development Server" });
                options.AddServer(new OpenApiServer { Url = "https://cabincrewcareer.azurewebsites.net", Description = "Production Server" });
            }
        }
    }
}
