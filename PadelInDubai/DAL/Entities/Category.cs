namespace PadelInDubai.DAL.Entities
{
    public class Category
    {
        public string Title { get; set; }
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public bool IsCategory { get; set; }
        public int SalonServiceId { get; set; }
    }
}
