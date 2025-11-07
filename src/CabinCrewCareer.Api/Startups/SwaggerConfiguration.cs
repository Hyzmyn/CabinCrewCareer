using Asp.Versioning.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace CabinCrewCareer.Api.Startups
{
    public static class SwaggerConfiguration
    {
        public static WebApplication ConfigureSwagger(this WebApplication app)
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                        c.DefaultModelExpandDepth(2);
                        c.DocExpansion(DocExpansion.None);
                        c.DisplayRequestDuration();
                    }
                    //c.SwaggerEndpoint("/swagger/v1/swagger.json", "CCC API");
                    //c.DefaultModelExpandDepth(2);
                    //c.DocExpansion(DocExpansion.None);
                    //c.DisplayRequestDuration();
                }); ;
            }

            return app;
        }
    }
}
