using Microsoft.AspNetCore.Mvc;
using PadelInDubai.Attributes;
using PadelInDubai.Services;
using PadelInDubai.Services.Interfaces;

namespace PadelInDubai.Controllers;

[ApiController]
[Route("[controller]")]
//[LocalhostOnly]
public class EventController(ILogger<EventController> logger, IEventService service, TelegramService tgService, IBookingService bookingService) : ControllerBase
{
    private readonly ILogger<EventController> _logger = logger;
    private readonly IEventService _service = service;
    private readonly TelegramService _tgService = tgService;
    private readonly IBookingService _bookingService = bookingService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(await _service.GetById(id));
    }

    [HttpPost("Sync")]
    public async Task<IActionResult> Sync()
    {
        await _service.Sync();
        return NoContent();
    }

    [HttpPost("SyncPastEvents")]
    public async Task<IActionResult> SyncPastEvents()
    {
        await _service.SyncPastDbEvents();
        return NoContent();
    }

    [HttpPost("SyncById")]
    public async Task<IActionResult> Sync(int id)
    {
        await _bookingService.SyncById(id, true);
        return NoContent();
    }

    [HttpPost("Send")]
    public async Task<IActionResult> Send()
    {
        var temp = await _service.GetAll();
        await _tgService.SendEventMessageAsync(temp.First());
        return NoContent();
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update()
    {
        var temp = await _service.GetAll();
        await _tgService.UpdateEventMessageAsync(temp.First(x => x.MessageId == 6));
        return NoContent();
    }

    [HttpPost("DeleteTgData")]
    public async Task<IActionResult> DeleteTgData()
    {
        await _service.DeleteTgData();
        return NoContent();
    }

    [HttpPost("DeleteAllTgMessages")]
    public async Task<IActionResult> DeleteAllTgMessages()
    {
        await _service.DeleteAllTgMessages();
        return NoContent();
    }
}

