using Microsoft.AspNetCore.Mvc;
using CFBSharp.Api;
using CFBSharp.Model;
using Microsoft.Extensions.Caching.Memory;

namespace CFBapi.Controllers;

[ApiController]
[Route("[controller]")]
public class GamesController : ControllerBase
{
    private readonly ILogger<GamesController> _logger;
    private readonly IMemoryCache _memoryCache;

    public GamesController(ILogger<GamesController> logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
    }

    [HttpGet()]
    public ICollection<Game> Get(int? year, int? week)
    {
        string step = "Initializing";
        ICollection<Game> response = new List<Game>();
            try
            {
                step = "Interpreting Query String";
                var _year = year ?? DateTime.Now.Year;
                int _week = week ?? 1;

                step = "Checking Cached Data";
                var cacheKey = $"Games_{year}_{week}";
                if(!_memoryCache.TryGetValue(cacheKey, out response))
                {
                    step = "Querying CFBD GamesApi.GetGames";
                    var cfbdGames = new GamesApi().GetGames(_year,_week);

                    step = "Parsing CFBD Response";
                    response = cfbdGames;

                    step = "Updating Cache";
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                    _memoryCache.Set(cacheKey, response, cacheEntryOptions);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while {step}:" + e.Message );
            }
            
            return response;
    }
}
