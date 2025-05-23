using System.ComponentModel.DataAnnotations;

namespace CapsuleAPI.DTO
{
    public class CapsuleDto
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Content { get; set; }

        public IFormFile? Image {  get; set; }
       
    }
}
