using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using PadelInDubai.BackgroundServices;
using PadelInDubai.DAL;
using PadelInDubai.HostedServices;
using PadelInDubai.Middlewares;
using PadelInDubai.Services;
using PadelInDubai.Services.Interfaces;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("PD_connection")));

builder.Services.AddScoped<ITelegramBotClient>(provider =>
    new TelegramBotClient(Environment.GetEnvironmentVariable("PD_Tg_bot_token")));
builder.Services.AddScoped<TelegramService>();
builder.Services.AddHttpClient();
// Register HttpClient and other services.
builder.Services.AddHttpClient<IExternalEventService, AltegioClient>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IExternalEventService, AltegioClient>();
builder.Services.AddSingleton<IScheduledApiService, ScheduledApiService>();
builder.Services.AddHostedService<EventSyncService>();
builder.Services.AddHostedService<TelegramBotHostedService>();
builder.Services.AddTransient<IMessagesHandler, TgMessagesHandler>();

// Register controllers.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver =
            new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
    });
// Add endpoints API explorer (helps Swagger generate API docs).
builder.Services.AddEndpointsApiExplorer();

// Add Swagger generator.
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure Swagger middleware only in development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggerMiddleware>();

//app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();
