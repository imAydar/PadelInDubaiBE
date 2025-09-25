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

        public async Task<IEnumerable<ClientDto>> GetClients(DateTimeOffset dateTime, Group type)
        {
            var evts = await _service.GetClients(dateTime, type);
            return evts;
        }
    }
}
