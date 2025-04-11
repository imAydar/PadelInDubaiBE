namespace PadelInDubai.DAL.Entities
{
    public class ClientTag
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; }
        public string EntitySlug { get; set; }
        public bool IsDeleted { get; set; }
    }
}
