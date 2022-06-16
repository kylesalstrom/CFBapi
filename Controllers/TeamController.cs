using Microsoft.AspNetCore.Mvc;
using CFBSharp.Api;
using CFBSharp.Model;
using Microsoft.Extensions.Caching.Memory;

namespace CFBapi.Controllers;

[ApiController]
[Route("[controller]")]
public class TeamController : ControllerBase
{
    private readonly ILogger<TeamController> _logger;
    private readonly IMemoryCache _memoryCache;

    public TeamController(ILogger<TeamController> logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
    }

    [HttpGet()]
    public List<Team> Get(int? year)
    {
        string step = "Initializing";
        List<Team> response = new List<Team>();
            try
            {
                step = "Interpreting Query String";
                var _year = year ?? DateTime.Now.Year;

                step = "Checking Cached Data";
                var cacheKey = $"FBSTeams_{year}";
                if(!_memoryCache.TryGetValue(cacheKey, out response))
                {
                    Console.WriteLine("Not Cached");
                    step = "Querying CFBD TeamsApi.GetFbsTeams";
                    var cfbdTeams = new TeamsApi().GetFbsTeams(_year);

                    step = "Parsing CFBD Response";
                    var teams = cfbdTeams.Select(t => {
                        var l = (dynamic)t.Location;
                        return new Team(){
                            Id = t.Id,
                            School = t.School,
                            Mascot = t.Mascot,
                            Abbreviation = t.Abbreviation,
                            AltName1 = t.AltName1,
                            AltName2 = t.AltName2,
                            AltName3 = t.AltName3,
                            Conference = t.Conference,
                            Division = t.Division,
                            Color = t.Color,
                            AltColor = t.AltColor,
                            Logos = t.Logos,
                            Location = new {
                                Capacity = (int?)l.capacity,
                                City = (string?)l.city,
                                CountryCode = (string?)l.country_code,
                                Dome = (bool?)l.dome,
                                Elevation = (double?)l.elevation,
                                Grass = (bool?)l.grass,
                                Name = (string?)l.name,
                                State = (string?)l.state,
                                Timezone = (string?)l.timezone,
                                Id = (int?)l.venue_id,
                                YearConstructed = (int?)l.year_constructed,
                                Zip = (int?)l.zip,
                                Latitude = (double?)l.latitude,
                                Longitude = (double?)l.longitude
                        }};
                    });

                    step = "Applying Fixes";
                    response = TeamFixes(teams);

                    step = "Updating Cache";
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                    _memoryCache.Set(cacheKey, response, cacheEntryOptions);
                }
                else
                {
                    Console.WriteLine("Cached!!!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while {step}:" + e.Message );
            }
            
            return response;
    }

    private List<Team> TeamFixes(IEnumerable<Team> teams)
    {
        var teamList = teams.ToList();

        teamList.Where(t => t.Id == 38).First().AltColor = "#000"; //Set Colorado AltColor to Black
        teamList.Where(t => t.Id == 213).First().AltColor = "#fff"; //Set Penn State AltColor to White
        teamList.Where(t => t.Id == 204).First().AltColor = "#000"; //Set Oregon State AltColor to Black
        teamList.Where(t => t.Id == 264).First().AltColor = "#B7A57A"; //Set Washington AltColor to Gold
        teamList.Where(t => t.Id == 2390).First().Color = "#F47321"; //Set Miami Color to Organge
        teamList.Where(t => t.Id == 2390).First().AltColor = "#005030"; //Set Miami AltColor to Green
        return teamList;
    }
}
