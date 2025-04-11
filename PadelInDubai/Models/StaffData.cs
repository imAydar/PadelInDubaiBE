namespace PadelInDubai.Models
{
    public class StaffData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public string Specialization { get; set; }
        public int? ApiId { get; set; }
        public int? UserId { get; set; }
        public double Rating { get; set; }
        public string? Prepaid { get; set; }
        public int ShowRating { get; set; }
        public int CommentsCount { get; set; }
        public int VotesCount { get; set; }
        public double AverageScore { get; set; }
        public string Avatar { get; set; }
        public string AvatarBig { get; set; }
    }

}
