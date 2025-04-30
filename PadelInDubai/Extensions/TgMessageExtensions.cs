using PadelInDubai.Models.Dtos;
using PadelInDubai.Models;
using System.Text.RegularExpressions;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using PadelInDubai.DAL.Entities;

namespace PadelInDubai.Extensions
{
    public static class TgMessageExtensions
    {

        public static string GetShortTitle(this Event evt)
        {
            return $"{evt.Service.Title.Substring(0, Math.Min(30, evt.Service.Title.Length))}. {evt.Date:HH:mm}";
        }

        public static (InlineKeyboardMarkup inlineKeyboard, string text) GetMessageParams(this EventDto evt, List<RecordData> records = null)
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

            return (inlineKeyboard, EscapeMarkdown(text));
        }

        private static string GetPlayersText(List<RecordData> records)
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

        private static string CreateTelegramMessage(EventDto eventDto)
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
[C абонементом](https://padelindubai.club/p/packs/) 135 AED
👥 Места: {freeSlotsCount} из {eventDto.Capacity}  

📌 Описание:
{eventDto.Comment}

📩 Запись: нажмите на кнопку и укажите уровень.

Возникнут вопросы? Пиши @padelindubai
";

            return message;
        }


        private static string EscapeMarkdown(string input)
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

        private static string EscapeNonLinkText(string text)
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
    }
}
