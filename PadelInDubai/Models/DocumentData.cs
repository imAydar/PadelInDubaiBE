namespace PadelInDubai.Models
{
    public class DocumentData
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public int VisitId { get; set; }
        public string TypeTitle { get; set; }
        public DateTime Date { get; set; }
        public string CategoryId { get; set; }
        public bool IsSaleBillPrinted { get; set; }
    }
}
