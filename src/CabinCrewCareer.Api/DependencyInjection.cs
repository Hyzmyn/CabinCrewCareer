using Asp.Versioning;
using CabinCrewCareer.Api.Startups;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace CabinCrewCareer.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services
                .AddSwaggerOptions()
                .AddEndpointsApiExplorer()
                .AddSwagger()
                .UseCors()
                .AddVersoning()
                .AddControllers();

            return services;
        }

        private static IServiceCollection AddSwaggerOptions(this IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            return services;
        }

        private static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                // using System.Reflection;
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                // Add file upload support
                options.OperationFilter<FileUploadOperationFilter>();

                //options.SwaggerDoc("v1", new OpenApiInfo
                //{
                //    Version = "v1",
                //    Title = "Cabin Crew Career API",
                //    Description = "A .NET Core Web API that allows you to work with recruitment and promotion process.",
                //    Contact = new OpenApiContact
                //    {
                //        Name = "GFA25SE113",
                //        Email = "example@email.com",
                //        Url = new Uri("https://github.com/thanhnhan151/CabinCrewCareer"),
                //    },
                //    License = new OpenApiLicense
                //    {
                //        Name = "MIT License",
                //        Url = new Uri("https://opensource.org/licenses/MIT")
                //    }
                //});

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer {accessToken}\"",
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        private static IServiceCollection UseCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });

            return services;
        }

        private static IServiceCollection AddVersoning(this IServiceCollection services)
        {
            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = ApiVersion.Default; //new ApiVersion(1, 0);
                o.ReportApiVersions = true;
                o.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("api-Version"),
                    new UrlSegmentApiVersionReader());
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
    }
}
