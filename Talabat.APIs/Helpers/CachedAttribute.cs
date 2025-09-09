using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Talabat.Core.Services.Contract;

namespace Talabat.APIs.Helpers
{
    public class CachedAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _timeToLiveInSeconds;

        public CachedAttribute(int timeToLiveInSeconds)
        {
            _timeToLiveInSeconds = timeToLiveInSeconds;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var responseCacheService =  context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();
            var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);
            var response = await responseCacheService.GetCachedResponseAsync(cacheKey);
            if (!string.IsNullOrEmpty(response)) {
                var result = new ContentResult()
                { Content = response,
                  ContentType = "application/json",
                  StatusCode = 200
                };
                context.Result = result;
                return;
            }
           var executedActionContext = await next.Invoke();
            if (executedActionContext.Result is OkObjectResult okObject&&okObject.Value is not null)
            {
                await responseCacheService.CacheResponseAsync(cacheKey, okObject.Value, TimeSpan.FromSeconds(_timeToLiveInSeconds));
            }
        }

        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append(request.Path);
            var sortedQuery = request.Query.OrderBy(x => x.Key);
            foreach (var (key,value) in sortedQuery)
            {
                keyBuilder.Append($"|{key}-{value}");
            }
            return keyBuilder.ToString();
        }
    }
}
 