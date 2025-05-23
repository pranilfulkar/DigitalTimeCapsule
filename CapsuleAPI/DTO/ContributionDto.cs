namespace CapsuleAPI.DTO
{
    public class ContributionDto
    {
        public int CapsuleId { get; set; }
        public string TextContent {  get; set; }
        public IFormFile? ImageFile { get; set; }
        public string ContributorId { get; set; }

    }
}
