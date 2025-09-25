using Microsoft.AspNetCore.Mvc;
using PadelInDubai.Mappings;
using PadelInDubai.Models.Dtos;
using PadelInDubai.Services.Interfaces;

namespace PadelInDubai.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoveableController : ControllerBase
    {
        private readonly IEventService _service;

        public LoveableController(IEventService service)
        {
            _service = service;
        }

        [HttpGet("ping")]
        public IActionResult Ping() => Ok("Alive");

        [HttpGet("GetClients")]
        public async Task<IActionResult> GetClients([FromQuery] DateTimeOffset dateTime, 
            [FromQuery] Group type)
        {
            var clients = await _service.GetClients(dateTime, type);
            return Ok(clients);
        }
    }
}
