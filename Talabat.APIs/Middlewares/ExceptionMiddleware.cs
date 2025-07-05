using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Talabat.APIs.Errors;

namespace Talabat.APIs.Middlewares
{
    public class ExceptionMiddleware : IMiddleware
    {
        //private readonly RequestDelegate _next;
        private readonly Microsoft.Extensions.Logging.ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(/*RequestDelegate next, */ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            //_next = next;
            _logger = logger;
            _env = env;
        }
        //public async Task InvokeAsync(HttpContext httpContext)
        //{
        //    try
        //    {
        //        await _next(httpContext);
        //    }
        //    catch (Exception ex)
        //    {

        //        _logger.LogError(ex.Message);

        //        httpContext.Response.StatusCode =(int) HttpStatusCode.InternalServerError;
        //        httpContext.Response.ContentType = "application/json";
        //        var response = _env.IsDevelopment() ? new ApiExceptionResponse((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace) : new ApiExceptionResponse((int)HttpStatusCode.InternalServerError);
        //        var options = new JsonSerializerOptions() {PropertyNamingPolicy=JsonNamingPolicy.CamelCase};
        //        var json = JsonSerializer.Serialize(response,options);
        //       await httpContext.Response.WriteAsync(json);

        //    }
        //}

        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate _next)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message);

                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                httpContext.Response.ContentType = "application/json";
                var response = _env.IsDevelopment() ? new ApiExceptionResponse((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace) : new ApiExceptionResponse((int)HttpStatusCode.InternalServerError);
                var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);
                await httpContext.Response.WriteAsync(json);

            }
        }
    }
}
