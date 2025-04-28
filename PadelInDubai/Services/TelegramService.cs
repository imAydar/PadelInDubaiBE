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

namespace PadelInDubai.Services
{
    public class TelegramService(ILogger<TelegramService> logger, ITelegramBotClient botClient, IEventRepository eventRepository)
    {
        private readonly ILogger<TelegramService> _logger = logger;

        private static readonly string _chatId = Environment.GetEnvironmentVariable("PD_TgChatId");
        private const int _gamesTopicId = 38;
        private const int _trainsTopicId = 37;
        private const bool _useTopics = true;

        private readonly ITelegramBotClient _botClient = botClient;
        private readonly IEventRepository _eventRepository = eventRepository;

        public async Task HandleUpdateAsync(Update update)
        {
            if (update.Type == UpdateType.Message && update.Message.Text == "/start")
            {
                var buttons = new InlineKeyboardMarkup(new[]
                {
            new[] { InlineKeyboardButton.WithCallbackData("🎾 Game", "choose_game") },
            new[] { InlineKeyboardButton.WithCallbackData("🏋️ Training", "choose_training") }
        });

                await _botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "Welcome! What would you like to do?",
                    replyMarkup: buttons
                );
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                //await HandleCallbackQueryAsync(update.CallbackQuery);
            }
        }

        public async Task SendEventMessageAsync(EventDto evt, bool pin = false)
        {
            var chatId = new ChatId(_chatId);

            var (inlineKeyboard, text) = GetMessageParams(evt);

            var caption = EscapeMarkdown(text);
            int? topicId = evt.Group == Mappings.Group.Game ? _gamesTopicId :
                             evt.Group == Mappings.Group.Train ? _trainsTopicId :
                                null;
            var message = await _botClient.SendPhoto(
                chatId: chatId,
                messageThreadId: _useTopics ? topicId.Value : null,
                photo: evt.Picture,
                caption: caption,
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

            await _eventRepository.UpdateMessage(evt.Id, message.MessageId, caption.GetHashCode());
        }

        public async Task UpdateEventMessageAsync(EventDto evt, List<RecordData> records = null)
        {
            var chatId = new ChatId(_chatId);
            var (inlineKeyboard, text) = GetMessageParams(evt, records);

            if (!evt.MessageId.HasValue)
            {
                throw new NullReferenceException();
            }

            var txt = EscapeMarkdown(text);
            if (txt.GetHashCode() == evt.TextHash)
            {
                return;
            }

            try
            {
                await _botClient.EditMessageCaption(
                    chatId: chatId,
                    messageId: evt.MessageId.Value,
                    caption: txt,
                    parseMode: ParseMode.MarkdownV2,
                    default, default,
                    replyMarkup: inlineKeyboard
                );

                await _eventRepository.UpdateMessage(evt.Id, evt.MessageId.Value, txt.GetHashCode());
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
                text: "Choose an option:",
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
                text: $"Choose a date for {selectedType}:",
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
                    var (inlineKeyboard, text) = GetMessageParams(evt.ToDto());

                    var caption = EscapeMarkdown(text);

                    var message = await _botClient.SendMessage(
                        chatId: query.Message.Chat.Id,
                        text: caption,
                        parseMode: ParseMode.MarkdownV2,
                        replyMarkup: inlineKeyboard
                    );
                }
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: query.Message.Chat.Id,
                    text: "No events found for that day."
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
                    InlineKeyboardButton.WithUrl("🎟️ Записаться", link)
                }
            });

            var text = CreateTelegramMessage(evt);
            text += GetPlayersText(evt.Records.ToList());

            return (inlineKeyboard, text);
        }

        private string GetPlayersText(List<RecordData> records)
        {
            if (records?.Any() != true)
            {
                return string.Empty;
            }

            var message = string.Empty + Environment.NewLine;
            var ind = 1;
            for (int i = 0; i < records.Count; i++)
            {
                //var confirmed = records[i].PaidFull == 1 ? "✅" : string.Empty;
                message += $"{ind++}.🎾 {records[i].Client.Name} {records[i].Client.Level}" + Environment.NewLine;
                if (records[i].ClientsCount > 1)
                {
                    for (int j = 1; j < records[i].ClientsCount; j++)
                    {
                        message += $"{ind++}.🎾 {records[i].Client.Name} +1" + Environment.NewLine;
                    }
                }
            }
            message += "—\r\n[Анонсы игр](https://t.me/padeldubai_games)";
            return message;
        }

        private string CreateTelegramMessage(EventDto eventDto)
        {
            var culture = new System.Globalization.CultureInfo("ru-RU");
            var formattedDate = eventDto.Date.ToString("dddd, dd MMMM", culture);
            var formattedTime = eventDto.Date.ToString("HH:mm");
            var recordsCount = eventDto.Records?.Sum(r => r.ClientsCount) ?? eventDto.RecordsCount;
            var freeSlotsCount = eventDto.Capacity - recordsCount < 0 ? 0 : eventDto.Capacity - recordsCount;
            //https://maps.app.goo.gl/UKwd6Hx1LQQZqN917
            var message = $@"
🎾 {eventDto.Title}
📅 Когда: {formattedDate} в {formattedTime}
📍 Где: [{eventDto.LocationName}]({eventDto.LocationUrl})
Для кого: [Определятор Уровня](https://forms.gle/svzhWNGx354VHjY27)
💰 Стоимость: {eventDto.PriceMax} AED
Текст абонемента[Абонемент](https://padelindubai.club/p/packs/)
👥 Места: {freeSlotsCount} из {eventDto.Capacity}  

📌 Описание:
{eventDto.Comment}

📩 Запись: нажмите на кнопку и укажите уровень.

Возникнут вопросы? Пиши @padelindubai
";

            return message;
        }


        private string EscapeMarkdown(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var linkRegex = new Regex(@"\[[^\]]+\]\([^)]+\)", RegexOptions.Compiled);
            var parts = linkRegex.Split(input);
            var matches = linkRegex.Matches(input);

            var sb = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                sb.Append(EscapeNonLinkText(parts[i]));
                if (i < matches.Count)
                    sb.Append(matches[i].Value);
            }
            return sb.ToString();
        }

        private string EscapeNonLinkText(string text)
        {
            return text
                .Replace("\\", "\\\\")
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
