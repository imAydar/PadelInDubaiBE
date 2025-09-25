namespace PadelInDubai.Models.Dtos
{
    public class ClientDto
    {
        public string Phone { get; set; }
        public string DisplayName { get; set; }
        public string? Level { get; set; }
        public int ClientsCount { get; internal set; }
    }
}
