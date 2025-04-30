using PadelInDubai.DAL;
using PadelInDubai.Extensions;
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
        private const int _gamesId = 10759477;
        private const int _trainsId = 10761747;

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
            else if (update.Type == UpdateType.Message && update.Message.Text == "📅 Расписание")
            {
                // No longer needed, do nothing or remove this block
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
                else if (data.StartsWith("back_to_days_"))
                {
                    int categoryId = int.Parse(data.Substring("back_to_days_".Length));
                    await ShowDaysOfWeekSelection(update.CallbackQuery, categoryId);
                }
                else if (data.StartsWith("back_to_date_"))
                {
                    // e.g. back_to_date_2025-03-29_10759477
                    var parts = data.Split('_');
                    var dateStr = parts[3];
                    var categoryId = int.Parse(parts[4]);
                    await EditDateSelection(update.CallbackQuery, dateStr, categoryId);
                }
                else if (data.StartsWith("event_"))
                {
                    var idStr = data.Substring("event_".Length);
                    if (int.TryParse(idStr, out int eventId))
                    {
                        await HandleEventSelection(update.CallbackQuery, eventId);
                    }
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
            int selectedCategoryId = query.Data == "type_games" ? _gamesId : _trainsId;
            var today = DateTime.Today;
            var culture = new System.Globalization.CultureInfo("ru-RU");
            var buttons = Enumerable.Range(0, 7)
                .Select(offset => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        today.AddDays(offset).ToString("dddd, dd MMM", culture),
                        $"date_{selectedCategoryId}_{today.AddDays(offset):yyyy-MM-dd}"
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
            await _botClient.EditMessageText(
                chatId: query.Message.Chat.Id,
                messageId: messageId,
                text: "Выберите тип:",
                replyMarkup: new InlineKeyboardMarkup(buttons)
            );
        }

        private async Task HandleDateSelection(CallbackQuery query)
        {
            var data = query.Data.Split('_'); // e.g. ["date", "10759477", "2025-03-29"]
            int categoryId = int.Parse(data[1]);
            var date = DateTime.Parse(data[2]);

            var events = await _eventRepository.GetByDate(date, categoryId);

            int messageId = _lastMessageIds[query.Message.Chat.Id];

            if (events.Any())
            {
                if (events.Count() == 1)
                {
                    var evt = events.First();
                    var (inlineKeyboard, text) = evt.ToDto().GetMessageParams();
                    // Add Back button as a new row
                    var keyboardRows = inlineKeyboard.InlineKeyboard.ToList();
                    keyboardRows.Add(new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"back_to_date_{evt.Date:yyyy-MM-dd}_{evt.Service.CategoryId}") });
                    var updatedKeyboard = new InlineKeyboardMarkup(keyboardRows);

                    await _botClient.EditMessageText(
                        chatId: query.Message.Chat.Id,
                        messageId: messageId,
                        text: text,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                        replyMarkup: updatedKeyboard
                    );
                }
                else
                {
                    var buttons = events.Select(evt => new[]
                    {
                        InlineKeyboardButton.WithCallbackData($"{evt.GetShortTitle()}", $"event_{evt.Id}")
                    }).ToList();
                    // Add Back button to date selection
                    buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"back_to_date_{date:yyyy-MM-dd}_{categoryId}") });

                    await _botClient.EditMessageText(
                        chatId: query.Message.Chat.Id,
                        messageId: messageId,
                        text: $"Выберите событие:",
                        replyMarkup: new InlineKeyboardMarkup(buttons)
                    );
                }
            }
            else
            {
                await _botClient.EditMessageText(
                    chatId: query.Message.Chat.Id,
                    messageId: messageId,
                    text: "Не удалось найти события на этот день.\n⬅️ Назад",
                    replyMarkup: new InlineKeyboardMarkup(new[] {
                        new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"back_to_type") }
                    })
                );
            }
        }

        private async Task ShowDaysOfWeekSelection(CallbackQuery query, int categoryId)
        {
            var today = DateTime.Today;
            var culture = new System.Globalization.CultureInfo("ru-RU");
            var buttons = Enumerable.Range(0, 7)
                .Select(offset => new[] {
                    InlineKeyboardButton.WithCallbackData(
                        today.AddDays(offset).ToString("dddd, dd MMM", culture),
                        $"date_{categoryId}_{today.AddDays(offset):yyyy-MM-dd}"
                    )
                }).ToList();
            // Add back to type selection
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", "back_to_type") });

            int messageId = _lastMessageIds[query.Message.Chat.Id];
            await _botClient.EditMessageTextAsync(
                chatId: query.Message.Chat.Id,
                messageId: messageId,
                text: $"Выберите дату:",
                replyMarkup: new InlineKeyboardMarkup(buttons)
            );
        }

        private async Task EditDateSelection(CallbackQuery query, string dateStr, int categoryId)
        {
            var date = DateTime.Parse(dateStr);

            var events = await _eventRepository.GetByDate(date, categoryId);

            int messageId = _lastMessageIds[query.Message.Chat.Id];

            if (events.Any())
            {
                var buttons = events.Select(evt => new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{evt.GetShortTitle()}", $"event_{evt.Id}")
                }).ToList();
                // Add Back button to go back to days of week for this category
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"back_to_days_{categoryId}") });

                await _botClient.EditMessageText(
                    chatId: query.Message.Chat.Id,
                    messageId: messageId,
                    text: $"Выберите событие:",
                    replyMarkup: new InlineKeyboardMarkup(buttons)
                );
            }
            else
            {
                await _botClient.EditMessageText(
                    chatId: query.Message.Chat.Id,
                    messageId: messageId,
                    text: "Не удалось найти события на этот день.",
                    replyMarkup: new InlineKeyboardMarkup(new[] {
                        new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"back_to_days_{categoryId}") }
                    })
                );
            }
        }

        private async Task HandleEventSelection(CallbackQuery query, int eventId)
        {
            var evt = await _eventRepository.GetByIdAsync(eventId);
            if (evt == null)
            {
                await _botClient.AnswerCallbackQuery(query.Id, "Событие не найдено.");
                return;
            }

            var (inlineKeyboard, text) = evt.ToDto().GetMessageParams();
            // Add Back button as a new row
            var keyboardRows = inlineKeyboard.InlineKeyboard.ToList();
            keyboardRows.Add(new[] { InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"back_to_date_{evt.Date:yyyy-MM-dd}_{evt.Service.CategoryId}") });
            var updatedKeyboard = new InlineKeyboardMarkup(keyboardRows);

            int messageId = _lastMessageIds[query.Message.Chat.Id];

            await _botClient.EditMessageText(
                chatId: query.Message.Chat.Id,
                messageId: messageId,
                text: text,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                replyMarkup: updatedKeyboard
            );
        }
    }
}
