using Microsoft.Extensions.Logging;
using PadelInDubai.Controllers;
using PadelInDubai.DAL;
using PadelInDubai.Mappings;
using PadelInDubai.Migrations;
using PadelInDubai.Models;
using PadelInDubai.Models.Dtos;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using PadelInDubai.Extensions;

namespace PadelInDubai.Services
{
    public class TelegramService(ILogger<TelegramService> logger, ITelegramBotClient botClient, IEventRepository eventRepository)
    {
        private readonly ILogger<TelegramService> _logger = logger;

        private static readonly string _chatId = Environment.GetEnvironmentVariable("PD_TgChatId");
        private const int _gamesTopicId = 4163;
        private const int _trainsTopicId = 2686;
        private const bool _useTopics = true;

        private readonly ITelegramBotClient _botClient = botClient;
        private readonly IEventRepository _eventRepository = eventRepository;

        //public async Task HandleUpdateAsync(Update update)
        //{
        //    if (update.Type == UpdateType.Message && update.Message.Text == "/start")
        //    {
        //        var buttons = new InlineKeyboardMarkup(new[]
        //        {
        //    new[] { InlineKeyboardButton.WithCallbackData("🎾 Игры", "choose_game") },
        //    new[] { InlineKeyboardButton.WithCallbackData("🏋️ Тренировки", "choose_training") }
        //});

        //        await _botClient.SendTextMessageAsync(
        //            chatId: update.Message.Chat.Id,
        //            text: "Добро пожаловать!",
        //            replyMarkup: buttons
        //        );
        //    }
        //    else if (update.Type == UpdateType.CallbackQuery)
        //    {
        //        //await HandleCallbackQueryAsync(update.CallbackQuery);
        //    }
        //}

        public async Task SendEventMessageAsync(EventDto evt, bool pin = false)
        {
            var chatId = new ChatId(_chatId);

            var (inlineKeyboard, text) = evt.GetMessageParams();

            int? topicId = evt.Group == Mappings.Group.Game ? _gamesTopicId :
                             evt.Group == Mappings.Group.Train ? _trainsTopicId :
                                null;
            var fileName = $"{evt.Picture}";
            var filePath = Path.Combine(AppContext.BaseDirectory, "Content", fileName);
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(AppContext.BaseDirectory, "Content", "Untitled.jpg");
            }
            using var stream = File.OpenRead(filePath);

            var message = await _botClient.SendPhoto(
                chatId: chatId,
                messageThreadId: _useTopics ? topicId.Value : null,
                photo: InputFile.FromStream(stream),
                caption: text,
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: inlineKeyboard
            );

            if (pin)
            {
                await _botClient.PinChatMessage(
                    chatId: chatId,
                    messageId: message.MessageId,
                    disableNotification: false
                );
            }

            await _eventRepository.UpdateMessage(evt.Id, message.MessageId, text.GetHashCode());
        }

        public async Task UpdateEventMessageAsync(EventDto evt, List<RecordData> records = null)
        {
            var chatId = new ChatId(_chatId);
            var (inlineKeyboard, text) = evt.GetMessageParams(records);

            if (!evt.MessageId.HasValue)
            {
                throw new NullReferenceException();
            }
            //if (txt.GetHashCode() == evt.TextHash)
            //{
            //    return;
            //}

            try
            {
                await _botClient.EditMessageCaption(
                    chatId: chatId,
                    messageId: evt.MessageId.Value,
                    caption: text,
                    parseMode: ParseMode.MarkdownV2,
                    default, default,
                    replyMarkup: inlineKeyboard
                );

                await _eventRepository.UpdateMessage(evt.Id, evt.MessageId.Value, text.GetHashCode());
            }
            catch (Exception ex)
            {
                //TODO: exception for 404.
                if (!ex.Message.Contains("there is no text in the message to edit") &&
                    !ex.Message.Contains("message is not modified"))
                {
                    throw;
                }
            }
        }

        private async Task HandleStart(Message message)
        {
            var buttons = new[]
                    {
                new[] {
                    InlineKeyboardButton.WithCallbackData("Games", "type_games"),
                    InlineKeyboardButton.WithCallbackData("Trainings", "type_trainings")
                }
            };

            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "Выберите тип события::",
                replyMarkup: new InlineKeyboardMarkup(buttons)
            );
        }

        private async Task HandleTypeSelection(CallbackQuery query)
        {
            string selectedType = query.Data == "type_games" ? "Games" : "Trainings";

            var today = DateTime.Today;
            var buttons = Enumerable.Range(0, 7)
                .Select(offset => new[] {
            InlineKeyboardButton.WithCallbackData(
                today.AddDays(offset).ToString("dddd, dd MMM"),
                $"date_{selectedType}_{today.AddDays(offset):yyyy-MM-dd}"
            )
                }).ToArray();

            await _botClient.SendMessage(
                chatId: query.Message.Chat.Id,
                text: $"Выберите день: {selectedType}:",
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
                .Where(e => e.Date >= startOfDay && e.Date < endOfDay && e.Service.Title == type);

            // .Where(e => e.Date)//FetchEventsAsync(type, date); // Your method here

            if (events.Any())
            {
                foreach (var evt in events)
                {
                    var (inlineKeyboard, text) = evt.ToDto().GetMessageParams();

                    var message = await _botClient.SendMessage(
                        chatId: query.Message.Chat.Id,
                        text: text,
                        parseMode: ParseMode.MarkdownV2,
                        replyMarkup: inlineKeyboard
                    );
                }
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: query.Message.Chat.Id,
                    text: "Не удалось найти события на этот день."
                );
            }
        }

        

        //Not working for msgs older than 2d.
        internal async Task DeleteMessages(IEnumerable<int> messageIds)
        {
            var chatId = new ChatId(_chatId);
            var me = await botClient.GetMe();
            var chatMember = await botClient.GetChatMember(chatId, me.Id);

            try
            {
                await _botClient.DeleteMessages(chatId: chatId, messageIds: messageIds);
            }
            catch (Exception ex)
            {
                foreach (var msgId in messageIds)
                {
                    try
                    {
                        await _botClient.DeleteMessage(chatId: chatId, messageId: msgId);
                    }
                    catch
                    {
                        var r = 0;
                    }
                }
            }
        }

        public async Task UnpinAll()
        {
            var chatId = new ChatId(_chatId);
            try
            {
                await _botClient.UnpinAllForumTopicMessages(chatId: chatId, _gamesTopicId);
                await _botClient.UnpinAllForumTopicMessages(chatId: chatId, _trainsTopicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Couldn't unpin the messages");
            }
        }
    }
}
