using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Talabat.APIs.Extensions;
using Talabat.APIs.Middlewares;
using Talabat.Core.Entities.Identity;
using Talabat.Infrastructure._Identinty;
using Talabat.Infrastructure._Identity;
using Talabat.Infrastructure.Data;

namespace Talabat.APIs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add services to the container.

            builder.Services.AddControllers().AddNewtonsoftJson(options =>{ options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; });
            builder.Services.AddCors(options => options.AddPolicy("MyPolicy", policyOptions => { policyOptions.AllowAnyHeader().AllowAnyMethod().WithOrigins(builder.Configuration["FrontBaseUrl"]); }));
            builder.Services.AddDbContext<StoreContext>(options=>options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>options.UseSqlServer(builder.Configuration.GetConnectionString("IdenetityConnection")));
            builder.Services.AddSingleton<IConnectionMultiplexer>((serviceprovider) => {
                return ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis"));
            });
            builder.Services.AddApplicationServicres();
            builder.Services.AddAuthService(builder.Configuration);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
           builder.Services.AddSwaggerServices();

            var app = builder.Build();

            using var scope = app.Services.CreateScope(); 

            var services = scope.ServiceProvider;

            StoreContext storeContext = services.GetRequiredService<StoreContext>();
            ApplicationIdentityDbContext applicationIdentityDbContext = services.GetRequiredService<ApplicationIdentityDbContext>();

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

                //var logger = loggerFactory.CreateLogger<Program>();
            try
            {
                await storeContext.Database.MigrateAsync();
                await StoreContextSeed.SeedAsync(storeContext);
                await applicationIdentityDbContext.Database.MigrateAsync();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                await ApplicationIdentityDataSeed.SeedUsersAsync(userManager);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "an error has been occured during apply the migration");
            }



            //app.UseMiddleware<ExceptionMiddleware>();
            //app.Use(async(httpContext, _next) =>
            //{
            //    try
            //    {
            //        await _next(httpContext);
            //    }
            //    catch (Exception ex)
            //    {

            //        logger.LogError(ex.Message);

            //        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            //        httpContext.Response.ContentType = "application/json";
            //        var response = app.Environment.IsDevelopment() ? new ApiExceptionResponse((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace) : new ApiExceptionResponse((int)HttpStatusCode.InternalServerError);
            //        var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            //        var json = JsonSerializer.Serialize(response, options);
            //        await httpContext.Response.WriteAsync(json);

            //    }

            //});
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerMiddleware();
               
            }
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseStatusCodePagesWithReExecute ("/errors/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("MyPolicy");
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
