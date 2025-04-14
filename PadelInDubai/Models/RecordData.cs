namespace PadelInDubai.Models
{
    public class RecordData
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int StaffId { get; set; }
        public List<ServiceData> Services { get; set; }
        public StaffData Staff { get; set; }
        public ClientData Client { get; set; }
        public int ClientsCount { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime CreateDate { get; set; }
        public string Comment { get; set; }
        public bool Online { get; set; }
        public int VisitAttendance { get; set; }
        public int Attendance { get; set; }
        public int Confirmed { get; set; }
        public int SeanceLength { get; set; }
        public int Length { get; set; }
        public int SmsBefore { get; set; }
        public int SmsNow { get; set; }
        public string SmsNowText { get; set; }
        public int EmailNow { get; set; }
        public int Notified { get; set; }
        public int MasterRequest { get; set; }
        public string ApiId { get; set; }
        public string FromUrl { get; set; }
        public int ReviewRequested { get; set; }
        public int VisitId { get; set; }
        public int CreatedUserId { get; set; }
        public bool Deleted { get; set; }
        public int PaidFull { get; set; }
        public bool Prepaid { get; set; }
        public bool PrepaidConfirmed { get; set; }
        public bool IsUpdateBlocked { get; set; }
        public DateTime LastChangeDate { get; set; }
        public string CustomColor { get; set; }
        public string CustomFontColor { get; set; }
        public List<RecordLabel> RecordLabels { get; set; }
        public int ActivityId { get; set; }
        //public List<object> CustomFields { get; set; }
        public List<DocumentData> Documents { get; set; }
        public int? SmsRemainHours { get; set; }
    }
}
