namespace PadelInDubai.DAL.Entities
{
    public class Staff
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public string? Specialization { get; set; }
        public double Rating { get; set; }
        public string? Avatar { get; set; }
        public string? AvatarBig { get; set; }

        public string? LocationUrl { get; set; }

        public ICollection<Event> Events { get; set; }
    }
}
