using CabinCrewCareer.Application.Common.Mappings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CabinCrewCareer.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfiles));

            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

                //options.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
                //options.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            #region Validators
            //services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
            //services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserCommandValidator>();
            //services.AddScoped<IValidator<CreateSchoolCommand>, CreateSchoolCommandValidator>();
            //services.AddScoped<IValidator<UpdateSchoolRequest>, UpdateSchoolCommandValidator>();
            //services.AddScoped<IValidator<CreateBlogCommand>, CreateBlogCommandValidator>();
            //services.AddScoped<IValidator<UpdateBlogRequest>, UpdateBlogCommandValidator>();
            //services.AddScoped<IValidator<ChangePasswordCommand>, ChangePasswordCommandValidator>();
            #endregion

            services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));
            return services;
        }
    }
}
