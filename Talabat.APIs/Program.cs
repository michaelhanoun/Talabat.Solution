using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Talabat.APIs.Extensions;
using Talabat.Infrastructure.Generic_Repository.Data;

namespace Talabat.APIs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<StoreContext>(options=>options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<IConnectionMultiplexer>((serviceprovider) => {
                return ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis"));
            });
            builder.Services.AddApplicationServicres();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
           builder.Services.AddSwaggerServices();

            var app = builder.Build();

            using var scope = app.Services.CreateScope(); 

            var services = scope.ServiceProvider;

            StoreContext storeContext = services.GetRequiredService<StoreContext>();

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

                //var logger = loggerFactory.CreateLogger<Program>();
            try
            {
                await storeContext.Database.MigrateAsync();
                await StoreContextSeed.SeedAsync(storeContext);
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
            app.UseStatusCodePagesWithReExecute ("/errors/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
