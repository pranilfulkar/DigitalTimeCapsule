using System.ComponentModel.DataAnnotations;
using CapsuleAPI.Data;

namespace CapsuleAPI.Model
{
    public class Contribution
    {
        public int Id { get; set; }
        [Required]
        public int CapsuleId { get; set; }
        public TimeCapsule Capsule { get; set; }

        [Required]
        public string UserId { get; set; }
        public AppUser User { get; set; }

        [MaxLength(500)]
        public string TextContent { get; set; }

        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
