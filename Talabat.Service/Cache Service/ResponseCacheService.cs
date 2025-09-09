using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using Talabat.Core.Services.Contract;

namespace Talabat.Application.Cache_Service
{
    public class ResponseCacheService : IResponseCacheService
    {
        private readonly IDatabase _database;

        public ResponseCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
          _database = connectionMultiplexer.GetDatabase();   
        }
        public async Task CacheResponseAsync(string key, object response, TimeSpan timeToLive)
        {
            if (response is null) return;
            var serializeoptions = new JsonSerializerOptions() {PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await _database.StringSetAsync(key,JsonSerializer.Serialize(response,serializeoptions),timeToLive);
        }

        public async Task<string?> GetCachedResponseAsync(string key)
        {
            var response = await _database.StringGetAsync(key);
            if(response.IsNullOrEmpty) return null;
            return response;
        }
    }
}
