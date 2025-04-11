namespace PadelInDubai.Models
{
    public class ServiceData
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public bool IsCategory { get; set; }
        public int SalonServiceId { get; set; }
        public string? Comment { get; set; }
        public decimal PriceMin { get; set; }
        public decimal PriceMax { get; set; }
        public string? Prepaid { get; set; }
        public int AbonementRestriction { get; set; }
        public CategoryData? Category { get; set; }
    }

}
