namespace CapsuleAPI.DTO
{
    public class CapsulesResponse
    {
        public List<CapsuleListViewDto> SealedCapsules { get; set; } = new List<CapsuleListViewDto>();
        public List<CapsuleListViewDto> UnlockedCapsules { get; set; } = new List<CapsuleListViewDto>();
        public List<CapsuleListViewDto> UnsealedCapsules { get; set; } = new List<CapsuleListViewDto>();
        public int TotalCount { get; set; }
    }
}