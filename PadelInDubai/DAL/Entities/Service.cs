namespace PadelInDubai.DAL.Entities
{
    public class Service
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public int SalonServiceId { get; set; }
        public string? Comment { get; set; }
        public decimal PriceMin { get; set; }
        public decimal PriceMax { get; set; }
        public Category? Category { get; set; }

        public ICollection<Event> Events { get; set; }
    }
}
