using Microsoft.AspNetCore.Mvc;
using CFBapi.Model;

namespace CFBapi.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{

    [HttpGet()]
    public Test Get()
    {
        return new Test("Hey Now!!");
    }
}
