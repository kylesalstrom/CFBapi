using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using CFBSharp.Api;
using CFBSharp.Model;

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

    [HttpGet(Name = "GetTeams")]
    public IEnumerable<Team> Get()
    {
            var year = 2021;

            try
            {
                return new TeamsApi().GetFbsTeams(year)
                    .Select(t => {
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
                                Capacity = (int)l.capacity,
                                City = (string)l.city,
                                CountryCode = (string)l.country_code,
                                Dome = (bool)l.dome,
                                Elevation = (double)l.elevation,
                                Grass = (bool)l.grass,
                                Name = (string)l.name,
                                State = (string)l.state,
                                Timezone = (string)l.timezone,
                                Id = (int)l.venue_id,
                                YearConstructed = (int)l.year_constructed,
                                Zip = (int)l.zip,
                                Latitude = (double)l.latitude,
                                Longitude = (double)l.longitude
                        }};
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception when calling TeamsApi.GetFbsTeams: " + e.Message );
                return new List<Team>();
            }
    }
}
