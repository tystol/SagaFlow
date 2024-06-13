using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SimpleMvcExample.Controllers.Api;

// [Authorize]
[ApiController]
[Route("api/[controller]")]
public class WeatherController: ControllerBase
{
    public ActionResult<List<WeatherDto>> Get()
    {
        return new List<WeatherDto>
        {
            new WeatherDto { Location = "New York", Temperature = 20.5, Condition = "Sunny" },
            new WeatherDto { Location = "London", Temperature = 15.2, Condition = "Cloudy" },
            new WeatherDto { Location = "Sydney", Temperature = 25.3, Condition = "Rainy" },
            new WeatherDto { Location = "Tokyo", Temperature = 22.4, Condition = "Windy" },
            new WeatherDto { Location = "Paris", Temperature = 18.1, Condition = "Sunny" }
        };
    }    
    
    
}

public class WeatherDto
{
    public string Location { get; set; }
    public double Temperature { get; set; }
    public string Condition { get; set; }
}