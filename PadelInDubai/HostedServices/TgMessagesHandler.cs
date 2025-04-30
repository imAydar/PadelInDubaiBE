using PadelInDubai.DAL;
using PadelInDubai.Mappings;
using PadelInDubai.Models;
using PadelInDubai.Models.Dtos;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PadelInDubai.HostedServices
{
    public class TgMessagesHandler : IMessagesHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IEventRepository _eventRepository;

        private readonly ConcurrentDictionary<long, int> _lastMessageIds = new ConcurrentDictionary<long, int>();

        public TgMessagesHandler(ITelegramBotClient botClient, IEventRepository eventRepository)
        {
            _botClient = botClient;
            _eventRepository = eventRepository;
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            _botClient.StartReceiving(
                async (bot, update, ct) =>
                {
                    await HandleUpdateAsync(update, ct);
                },
                async (bot, exception, ct) =>
                {
                    Console.WriteLine($"Bot error: {exception.Message}");
                    await Task.CompletedTask;
                },
                cancellationToken: cancellationToken
            );

            Console.WriteLine("Telegram Bot is running...");

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }

        public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message.Text == "/start")
            {
                await HandleStart(update.Message);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                var data = update.CallbackQuery.Data;
                if (data.StartsWith("type_"))
                    await HandleTypeSelection(update.CallbackQuery);
                else if (data.StartsWith("date_"))
                    await HandleDateSelection(update.CallbackQuery);
                else if (data == "back_to_type")
                    await EditTypeSelection(update.CallbackQuery);
                else if (data.StartsWith("back_to_date_"))
                {
                    // e.g. back_to_date_2025-03-29_Games
                    var parts = data.Split('_');
                    var dateStr = parts[3];
                    var type = parts[4];
                    await EditDateSelection(update.CallbackQuery, dateStr, type);
                }
            }
        }

        private async Task HandleStart(Message message)
        {
            var buttons = new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Игры", "type_games"),
                    InlineKeyboardButton.WithCallbackData("Тренировки", "type_trainings")
                }
            };

            var sent = await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Выберите тип:",
                replyMarkup: new InlineKeyboardMarkup(buttons)
            );
            _lastMessageIds[message.Chat.Id] = sent.MessageId;
        }

        private async Task HandleTypeSelection(CallbackQuery query)
        {
            string selectedType = query.Data == "type_games" ? "Games" : "Trainings";
            var today = DateTime.Today;
            var buttons = Enumerable.Range(0, 7)
                .Select(offset => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        today.AddDays(offset).ToString("dddd, dd MMM"),
                        $"date_{selectedType}_{today.AddDays(offset):yyyy-MM-dd}"
                    )
                })
                .ToList();
            // Add Back button
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", "back_to_type") });

            int messageId = _lastMessageIds[query.Message.Chat.Id];
            await _botClient.EditMessageTextAsync(
                chatId: query.Message.Chat.Id,
                messageId: messageId,
                text: $"Выберите дату:",
                replyMarkup: new InlineKeyboardMarkup(buttons)
            );
        }

        private async Task EditTypeSelection(CallbackQuery query)
        {
            var buttons = new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Игры", "type_games"),
                    InlineKeyboardButton.WithCallbackData("Тренировки", "type_trainings")
                }
            };
            int messageId = _lastMessageIds[query.Message.Chat.Id];
            await _botClient.EditMessageTextAsync(
                chatId: query.Message.Chat.Id,
                messageId: messageId,
                text: "Выберите тип:",
                replyMarkup: new InlineKeyboardMarkup(buttons)
            );
        }

        private async Task HandleDateSelection(CallbackQuery query)
        {
            var data = query.Data.Split('_'); // e.g. ["date", "Games", "2025-03-29"]
            string type = data[1];
            var date = DateTime.Parse(data[2]);
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var events = (await _eventRepository.GetAllAsync())
                .Where(e => e.Date >= startOfDay && e.Date < endOfDay && e.Service.CategoryId == 10759477);

            int messageId = _lastMessageIds[query.Message.Chat.Id];

            if (events.Any())
            {
                var buttons = events.Select(evt => new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{evt.Service.Title} {evt.Date:HH:mm}", $"event_{evt.Id}")
                }).ToList();
                // Add Back button to date selection
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"back_to_date_{date:yyyy-MM-dd}_{type}") });

                await _botClient.EditMessageTextAsync(
                    chatId: query.Message.Chat.Id,
                    messageId: messageId,
                    text: $"Выберите событие:",
                    replyMarkup: new InlineKeyboardMarkup(buttons)
                );
            }
            else
            {
                await _botClient.EditMessageTextAsync(
                    chatId: query.Message.Chat.Id,
                    messageId: messageId,
                    text: "Не удалось найти события на этот день.\n⬅️ Назад",
                    replyMarkup: new InlineKeyboardMarkup(new[] {
                        new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"back_to_type") }
                    })
                );
            }
        }

        private async Task EditDateSelection(CallbackQuery query, string dateStr, string type)
        {
            var date = DateTime.Parse(dateStr);
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var events = (await _eventRepository.GetAllAsync())
                .Where(e => e.Date >= startOfDay && e.Date < endOfDay && e.Service.CategoryId == 10759477);

            int messageId = _lastMessageIds[query.Message.Chat.Id];

            if (events.Any())
            {
                var buttons = events.Select(evt => new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{evt.Service.Title} {evt.Date:HH:mm}", $"event_{evt.Id}")
                }).ToList();
                // Add Back button to date selection
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"back_to_type") });

                await _botClient.EditMessageTextAsync(
                    chatId: query.Message.Chat.Id,
                    messageId: messageId,
                    text: $"Выберите событие:",
                    replyMarkup: new InlineKeyboardMarkup(buttons)
                );
            }
            else
            {
                await _botClient.EditMessageTextAsync(
                    chatId: query.Message.Chat.Id,
                    messageId: messageId,
                    text: "Не удалось найти события на этот день.\n⬅️ Назад",
                    replyMarkup: new InlineKeyboardMarkup(new[] {
                        new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"back_to_type") }
                    })
                );
            }
        }

        private (InlineKeyboardMarkup inlineKeyboard, string text) GetMessageParams(EventDto evt, List<RecordData> records = null)
        {
            var link = $"https://b818310.alteg.io/company/768552/activity/info/{evt.Id}";
            // TODO: remove btn if date is greater than now.
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithUrl("🎟️ Записаться", link) // Replace with your actual event link
                }
            });

            // Format the message text
            var text = CreateTelegramMessage(evt);
            text += GetPlayersText(evt.Records?.ToList());

            return (inlineKeyboard, text);
        }

        private string GetPlayersText(List<RecordData> records)
        {
            if (records?.Any() != true)
            {
                return string.Empty;
            }

            var message = string.Empty + Environment.NewLine;

            for (int i = 0; i < records.Count; i++)
            {
                var confirmed = records[i].Confirmed == 1 ? "✅" : string.Empty;
                message += $"{i + 1}.🎾 {records[i].Client.Name} {records[i].Client.Level} {confirmed}" + Environment.NewLine;
            }

            return message;
        }

        private string CreateTelegramMessage(EventDto eventDto)
        {
            var culture = new System.Globalization.CultureInfo("ru-RU");
            var formattedDate = eventDto.Date.ToString("dddd, dd MMMM", culture);
            var formattedTime = eventDto.Date.ToString("HH:mm");

            var message = $@"
🎾 {eventDto.Title}
📅 Когда: {formattedDate} в {formattedTime} 
📍 Где: {eventDto.LocationName}  
💰 Стоимость: {eventDto.PriceMax} AED  
👥 Места: {eventDto.RecordsCount} из {eventDto.Capacity}  

📌 Описание:
{eventDto.Comment}

📩 Как записаться: Нажмите на кнопку Записаться и укажите уровень в комментарии.  
";

            return message;
        }

        // Helper method to escape Markdown characters.
        private string EscapeMarkdown(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return text
                .Replace("_", "\\_")
                .Replace("*", "\\*")
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("~", "\\~")
                .Replace("`", "\\`")
                .Replace(">", "\\>")
                .Replace("#", "\\#")
                .Replace("+", "\\+")
                .Replace("-", "\\-")
                .Replace("=", "\\=")
                .Replace("|", "\\|")
                .Replace("{", "\\{")
                .Replace("}", "\\}")
                .Replace(".", "\\.")
                .Replace("!", "\\!");
        }
    }
}
