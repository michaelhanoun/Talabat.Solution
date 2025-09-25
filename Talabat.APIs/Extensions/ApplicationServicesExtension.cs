using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Talabat.APIs.Errors;
using Talabat.APIs.Helpers;
using Talabat.APIs.Middlewares;
using Talabat.Application.AuthService;
using Talabat.Application.Cache_Service;
using Talabat.Application.Order_Service;
using Talabat.Application.Payment_Service;
using Talabat.Application.Product_Service;
using Talabat.Application.SendEmailService;
using Talabat.Core;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services.Contract;
using Talabat.Infrastructure;
using Talabat.Infrastructure._Identinty;
using Talabat.Infrastructure.Basket_Repository;

namespace Talabat.APIs.Extensions
{
    public static class ApplicationServicesExtension
    {
       
            public static IServiceCollection AddApplicationServicres(this IServiceCollection services)
            {
            services.AddSingleton(typeof(IResponseCacheService),typeof(ResponseCacheService));
             services.AddScoped(typeof(IPaymentService), typeof(PaymentService));
             services.AddScoped(typeof(IUnitOfWork),typeof(UnitOfWork));
             services.AddScoped(typeof(IAuthUnitOfWork),typeof(AuthUnitOfWork));
             services.AddScoped(typeof(IProductService),typeof(ProductService));
             services.AddScoped(typeof(IOrderService),typeof(OrderService));
            //services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
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
            services.AddIdentity<ApplicationUser, IdentityRole>(options => { options.User.RequireUniqueEmail=true; }).AddEntityFrameworkStores<ApplicationIdentityDbContext>().AddDefaultTokenProviders();
            services.AddScoped(typeof(IAuthService), typeof(AuthService));
            services.AddScoped(typeof(ISendEmail), typeof(SendEmail));
            return services;
        }
        public static IServiceCollection AddAuthService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(/*JwtBearerDefaults.AuthenticationScheme*/options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JWT:ValidIssuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:ValidAudience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:AuthKey"] ?? string.Empty)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:CliectSecret"];
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("https://www.googleapis.com/auth/user.phonenumbers.read");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                options.ClaimActions.MapJsonKey(ClaimTypes.MobilePhone, "phoneNumber");
            })
            ;

            return services;
        }

    }
}
