using PadelInDubai.Models.Dtos;
using PadelInDubai.Models;
using System.Text.RegularExpressions;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using PadelInDubai.DAL.Entities;
using Telegram.Bot.Types;

namespace PadelInDubai.Extensions
{
    public static class TgMessageExtensions
    {
        public async static Task<InputFileStream> GetPhoto(this EventDto evt)
        {
            /*using var httpClient = new HttpClient();
            using var imageStream = await httpClient.GetStreamAsync(evt.Picture);
            var inputFile = InputFile.FromStream(imageStream);//, "photo.jpg");
            return inputFile;*/
            var fileName = $"{evt.Picture}";
            var filePath = Path.Combine(AppContext.BaseDirectory, "Content", fileName);

            using var stream = System.IO.File.OpenRead(filePath);
            return InputFileStream.FromStream(stream);
        }

        public static string GetLevel(this EventDto evt)
        {
            var match = Regex.Match(evt.Title, @"\b(\w+)\s*\([^)]+\)");
            if (match.Success)
            {
                return match.Value;
            }

            var startIndex = evt.Title.IndexOf('(');
            var endIndex = evt.Title.IndexOf(')');

            if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
                return string.Empty;

            return evt.Title.Substring(startIndex + 1, endIndex - startIndex - 1);
        }

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
📅 {formattedDate} в {formattedTime}
📍 [{eventDto.LocationName}]({eventDto.LocationUrl})
💪 {eventDto.GetLevel()}
[Определятор Уровня](https://forms.gle/svzhWNGx354VHjY27)
💰 {eventDto.PriceMax} AED
[C абонементом](https://padelindubai.club/p/packs/) 135 AED
👥 Места: {freeSlotsCount} из {eventDto.Capacity}  

📌 {eventDto.Comment}
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
