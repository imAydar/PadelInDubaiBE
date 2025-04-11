using PadelInDubai.Mappings;

namespace PadelInDubai.Models.Dtos
{
    public class EventDto
    {
        public int RecordsCount;

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Capacity { get; set; }

        public string Title { get; set; }
        public decimal PriceMin { get; set; }
        public decimal PriceMax { get; set; }
        public string Comment { get; set; }

        public string LocationName { get; set; }
        public string Picture { get; set; }
        public int? MessageId { get; set; }
        public int? TextHash { get; set; }
        public Group Group { get; internal set; }
        public ICollection<RecordData> Records { get; internal set; }
        public string? LocationUrl { get; internal set; }
    }
}