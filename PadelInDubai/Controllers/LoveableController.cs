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

        [HttpGet]
        public async Task<IEnumerable<ClientDto>> GetClients([FromQuery] DateTimeOffset dateTime, 
            [FromQuery] Group type)
        {
            var clients = await _service.GetClients(dateTime, type);
            return clients;
        }
    }
}
