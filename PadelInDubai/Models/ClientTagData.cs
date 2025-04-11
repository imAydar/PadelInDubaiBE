namespace PadelInDubai.Models
{
    public class ClientTagData
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
        public string EntitySlug { get; set; }
        public bool IsDeleted { get; set; }
    }
}
