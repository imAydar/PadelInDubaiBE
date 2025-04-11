namespace PadelInDubai.DAL.Entities
{
    public class Event
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int CompanyId { get; set; }
        public int StaffId { get; set; }
        public DateTime Date { get; set; }
        public long Timestamp { get; set; }
        public int Length { get; set; }
        public int Capacity { get; set; }
        public string? Comment { get; set; }
        public int RecordsCount { get; set; }
        public Staff Staff { get; set; }
        public Service Service { get; set; }

        public int? MessageId { get; set; }
        public int? TextHash { get; internal set; }

        public bool LastUpdate { get; internal set; }

        public ICollection<Record> Records { get; set; }
    }
}
