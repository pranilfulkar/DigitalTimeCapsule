using System.ComponentModel.DataAnnotations;

namespace CapsuleAPI.DTO
{
    public class InviteContributorDTO
    
    {
        public int CapsuleId { get; set; }
        
        [Required(ErrorMessage = "The CollaboratorEmail field is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string CollaboratorEmail { get; set; }
    }
}
