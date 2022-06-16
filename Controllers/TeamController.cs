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

    public TeamController(ILogger<TeamController> logger)
    {
        _logger = logger;
    }

    [HttpGet()]
    public List<Team> Get(int? year)
    {
        string step = "Initializing";
            try
            {
                step = "Interpreting Query String";
                var _year = year ?? DateTime.Now.Year;

                step = "Querying CFBD TeamsApi.GetFbsTeams";
                var teams = new TeamsApi().GetFbsTeams(_year);

                step = "Parsing CFBD Response";
                var response = teams.Select(t => {
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
                return TeamFixes(response);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while {step}:" + e.Message );
                return new List<Team>();
            }
    }

    private List<Team> TeamFixes(IEnumerable<Team> teams)
    {
        var teamList = teams.ToList();

        teamList.Where(t => t.Id == 213).First().AltColor = "#fff"; //Set Penn State AltColor to White
        teamList.Where(t => t.Id == 204).First().AltColor = "#000"; //Set Oregon State AltColor to Black
        teamList.Where(t => t.Id == 264).First().AltColor = "#B7A57A"; //Set Washington AltColor to Gold
        teamList.Where(t => t.Id == 2390).First().Color = "#F47321"; //Set Miami Color to Organge
        teamList.Where(t => t.Id == 2390).First().AltColor = "#005030"; //Set Miami AltColor to Green
        return teamList;
    }
}
