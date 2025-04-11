namespace PadelInDubai.DAL.Entities
{
    public class Record
    {
        public string? Status { get; set; }

        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int StaffId { get; set; }
        public List<Service> Services { get; set; }
        public Staff Staff { get; set; }
        public Client Client { get; set; }
        public int ClientsCount { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime CreateDate { get; set; }
        public string? Comment { get; set; }
        public bool Online { get; set; }
        public int Confirmed { get; set; }
        public int Notified { get; set; }
        public string? FromUrl { get; set; }
        public int VisitId { get; set; }
        public int CreatedUserId { get; set; }
        public bool Deleted { get; set; }
        public int PaidFull { get; set; }
        public bool Prepaid { get; set; }
        public bool PrepaidConfirmed { get; set; }
        public DateTime LastChangeDate { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; }
        public int? ClientId { get; set; }
    }
}
