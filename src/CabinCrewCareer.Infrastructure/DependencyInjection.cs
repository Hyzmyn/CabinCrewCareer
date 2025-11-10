using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Security;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using CabinCrewCareer.Infrastructure.Security;
using CabinCrewCareer.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CabinCrewCareer.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddHttpContextAccessor()
                .AddSecurity(configuration)
                .AddServices()
                .AddPersistence(configuration);

            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<AuditableEntitiesInterceptor>();
            services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUploadFileService, UploadFileService>();
            services.AddScoped<IEmailService, EmailService>();
            // Use Cloudinary instead of Firebase Storage (free tier)
            services.AddScoped<IFirebaseStorageService, CloudinaryStorageService>();

            return services;
        }

        private static IServiceCollection AddSecurity(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings();
            configuration.Bind(JwtSettings.Section, jwtSettings);

            services.AddSingleton(Options.Create(jwtSettings));
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

            services
                .AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret))
                });

            return services;
        }

        private static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddDbContext<CabinCrewCareerDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DeployConnection"));
            });
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
