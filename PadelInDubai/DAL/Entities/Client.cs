namespace PadelInDubai.DAL.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int SuccessVisitsCount { get; set; }
        public int FailVisitsCount { get; set; }
        public string Level { get; set; }

        public ICollection<ClientTag> ClientTags { get; set; }
    }
}
