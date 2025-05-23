namespace CapsuleAPI.DTO
{
    public class CapsuleListViewDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime? UnlockDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public List<ContributionViewDto> Contributions { get; set; }
    }
}