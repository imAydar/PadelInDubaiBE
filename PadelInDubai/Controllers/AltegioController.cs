using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PadelInDubai.DAL;
using PadelInDubai.Mappings;
using PadelInDubai.Migrations;
using PadelInDubai.Models;
using PadelInDubai.Services;
using PadelInDubai.Services.Interfaces;

namespace PadelInDubai.Controllers
{
    [ApiController]
    [Route("/")]
    public class AltegioController(ILogger<EventController> logger,  IBookingService service) : ControllerBase
    {
        private readonly ILogger<EventController> _logger = logger;
        private readonly IBookingService _service = service;

        [HttpGet("ping")]
        public async Task<IActionResult> TestConnection()
        {
            return Ok("All good");
        }

        [HttpPost("test-from-files")]
        public async Task<IActionResult> TestWebhookFromFiles()
        {
            string folderPath = "C:\\Work\\git\\sttest\\njs\\logs";

            if (!Directory.Exists(folderPath))
            {
                return NotFound("TestWebhooks folder not found.");
            }

            var files = Directory.GetFiles(folderPath, "*.json");
            if (files.Length == 0)
            {
                return NotFound("No JSON webhook files found.");
            }

            foreach (var file in files)
            {
                try
                {
                    string json = System.IO.File.ReadAllText(file);
                    if (json.Contains("finances_operation"))
                    {
                        continue;
                    }
                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new SnakeCaseNamingStrategy()
                        },
                        NullValueHandling = NullValueHandling.Ignore
                    };

                    settings.Converters.Add(new FlexibleDateTimeConverter());

                    var request = JsonConvert.DeserializeObject<WebhookRequest>(json, settings);
                    //var request = await DeserializeResponseAsync(json);

                    await HandleWebhook(request.Body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing test webhook file: {file}");
                }
            }

            return Ok("Test webhook files processed.");
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] WebhookBody request)
        {
            _logger.LogInformation("Altegio webhook received. Status: {Status}, Record ID: {Id}, Client: {ClientName}",
                request.Status, request.Data?.Id, request.Data?.Client?.DisplayName);

            if (request.CompanyId != AltegioClient.CompanyId)
            {
                _logger.LogWarning($"Wrong company Id {request.CompanyId}");
                return Ok();
            }
            switch (request.Status)
            {
                case "create":
                    _logger.LogInformation("New booking created for {Client} on {Date}",
                        request.Data?.Client?.DisplayName, request.Data?.DateTime);
                    await _service.Sync(request.Data);
                    break;

                case "update":
                    _logger.LogInformation("New booking created for {Client} on {Date}",
                        request.Data?.Client?.DisplayName, request.Data?.DateTime);
                    await _service.Sync(request.Data);
                    //await _repository.Update(request.Data);
                    break;

                case "delete":
                    _logger.LogInformation("Booking deleted: {Id}", request.Data?.Id);
                    await _service.Sync(request.Data);
                    //await _repository.Delete(request.Data);
                    break;

                default:
                    _logger.LogWarning("Unknown status received: {Status}", request.Status);
                    break;
            }

            return Ok();
        }

        private async Task<WebhookRequest> DeserializeResponseAsync(string json)
        {
            var settings = new JsonSerializerSettings
            {
                // Use SnakeCaseNamingStrategy so JSON "service_id" maps to ServiceId.
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore
            };
            settings.Converters.Add(new CustomDateTimeConverter());
            return JsonConvert.DeserializeObject<WebhookRequest>(json, settings);
        }
    }
}
