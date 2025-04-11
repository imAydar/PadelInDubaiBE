namespace PadelInDubai.Models
{
    public class EventData
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int CompanyId { get; set; }
        public int StaffId { get; set; }
        public DateTime Date { get; set; }
        public long Timestamp { get; set; }
        public int Length { get; set; }
        public int Capacity { get; set; }
        public string Color { get; set; }
        public string Instructions { get; set; }
        public string StreamLink { get; set; }
        public string FontColor { get; set; }
        public bool Notified { get; set; }
        public string Comment { get; set; }
        public int RecordsCount { get; set; }
        public string Prepaid { get; set; }
        public StaffData Staff { get; set; }
        public ServiceData Service { get; set; }

    }
}
