using Swashbuckle.AspNetCore.SwaggerUI;

namespace CabinCrewCareer.Api.Startups
{
    public static class SwaggerConfiguration
    {
        public static WebApplication ConfigureSwagger(this WebApplication app)
        {
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CCC API");
                    c.DefaultModelExpandDepth(2);
                    c.DocExpansion(DocExpansion.None);
                    c.DisplayRequestDuration();
                }); ;
            }

            return app;
        }
    }
}
