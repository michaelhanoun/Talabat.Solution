using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Talabat.APIs.Errors;
using Talabat.APIs.Helpers;
using Talabat.APIs.Middlewares;
using Talabat.Core.Repositories.Contract;
using Talabat.Infrastructure.Basket_Repository;
using Talabat.Infrastructure.Generic_Repository;

namespace Talabat.APIs.Extensions
{
    public static class ApplicationServicesExtension
    {
       
            public static IServiceCollection AddApplicationServicres(this IServiceCollection services)
            {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddAutoMapper(typeof(MappingProfile));
            services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = (actionContext) =>
            {
                var errors = actionContext.ModelState.Where(P => P.Value.Errors.Count > 0).SelectMany(P => P.Value.Errors).Select(E => E.ErrorMessage).ToList();
                var response = new ApiValidationErrorResponse()
                {
                    Errors = errors
                };
                return new BadRequestObjectResult(response);
            });
            services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));
            services.AddTransient<ExceptionMiddleware>();
            return services;
        }
   
    }
}
