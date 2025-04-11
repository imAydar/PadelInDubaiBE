namespace PadelInDubai.Models
{
    public class ClientData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public int SuccessVisitsCount { get; set; }
        public int FailVisitsCount { get; set; }
        public List<ClientTagData> ClientTags { get; set; }
        public string Level { get; internal set; }
    }
}
