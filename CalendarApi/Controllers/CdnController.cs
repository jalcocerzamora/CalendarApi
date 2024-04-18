using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace CalendarApi.Controllers
{
    [ApiController]
    [Route("cdn")]
    public class CdnController : ControllerBase
    {
        [EnableCors]
        [HttpGet("calendar")]
        public IActionResult Get([FromQuery] string site, [FromQuery] string domain)
        {
            try
            {
                if (!string.IsNullOrEmpty(site) && !string.IsNullOrEmpty(domain))
                {
                    string pathFile = Path.Combine("cdn", $"{site}-{domain}", "horarios.json");
                    if (System.IO.File.Exists(pathFile))
                    {
                        string data = System.IO.File.ReadAllText(pathFile);
                        return Content(data, "application/json");
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }
        private readonly string _jsonFilePath = "horarios.json";

        [EnableCors]
        [HttpPost("calendar")]
        public async Task<IActionResult> Post([FromBody] JsonDocument jsonData, [FromQuery] string site, [FromQuery] string domain)
        {

            try
            {
                if (!string.IsNullOrEmpty(site) && !string.IsNullOrEmpty(domain))
                {
                    string pathFile = Path.Combine("cdn", $"{site}-{domain}", "horarios.json");

                    string jsonString = JsonSerializer.Serialize(jsonData);
                    await System.IO.File.WriteAllTextAsync(pathFile, jsonString);
                    return Ok(jsonString);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

    }

    
}
