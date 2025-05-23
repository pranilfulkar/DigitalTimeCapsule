using CapsuleAPI.DTO;
using CapsuleAPI.Interface;
using CapsuleAPI.Model;
using CapsuleAPI.Repository;

namespace CapsuleAPI.Service
{
    public class CapsuleService : ICapsuleService
    {
        private readonly CapsuleRepo _repo;

        public CapsuleService(CapsuleRepo repo)
        {
            _repo = repo;
        }

        public async Task<bool> CreateCapsuleAsync(CapsuleDto capsuleDto, string userId)
        {
            string imageUrl = null;
            if (capsuleDto.Image != null && capsuleDto.Image.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{capsuleDto.Image.FileName}";
                var folderPath = Path.Combine("wwwroot", "images", "capsules");
                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await capsuleDto.Image.CopyToAsync(stream);
                }

                imageUrl = $"/images/capsules/{fileName}";
            }

            var capsule = new TimeCapsule
            {
                Title = capsuleDto.Title,
                Description = capsuleDto.Description,
                Content = capsuleDto.Content,
                UnlockDate = null,
                UserId = userId,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(capsule);
            return true;
        }

        public async Task<(List<CapsuleListViewDto> capsules, int totalCount)> GetUserCapsulesAsync(string userId, string sortBy = "unlockDate", string sortDirection = "desc", int pageIndex = 0, int pageSize = 10, string? search = null)
        {
            var (capsules, totalCount) = await _repo.GetCapsuleByUserAsync(userId, sortBy, sortDirection, pageIndex, pageSize, search);

            var capsuleDto = capsules.Select(c => new CapsuleListViewDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Content = c.Content,
                ImageUrl = c.ImageUrl,
                UnlockDate = c.UnlockDate,
                CreatedAt = c.CreatedAt,
                Status = !c.UnlockDate.HasValue ? "unsealed" :
                         c.UnlockDate > DateTime.UtcNow ? "sealed" : "unlocked",
                Contributions = c.Contributions?.Select(con => new ContributionViewDto
                {
                    TextContent = con.TextContent,
                    ImageUrl = con.ImageUrl,
                    CreatedAt = con.CreatedAt,
                    collaboratorEmail = con.User?.Email ?? "Anonymous"
                }).ToList()
            }).ToList();

            return (capsuleDto, totalCount);
        }

        public async Task<bool> InviteContributorAsync(InviteContributorDTO inviteDto, string createrUserId)
        {
            return await _repo.InviteContributorAsync(inviteDto, createrUserId);
        }

        public async Task<bool> AddContributionAsync(ContributionDto contributionDto, string userId, string? imageUrl)
        {
            var capsule = await _repo.GetCapsuleByIdAsync(contributionDto.CapsuleId);
            if (capsule == null || capsule.UnlockDate.HasValue)
            {
                return false;
            }

            var isCreator = capsule.UserId == userId;
            var isContributor = await _repo.IsUserContributorAsync(contributionDto.CapsuleId, userId);

            if (!isCreator && !isContributor)
            {
                return false;
            }

            var contribution = new Contribution
            {
                CapsuleId = contributionDto.CapsuleId,
                UserId = userId,
                TextContent = contributionDto.TextContent,
                ImageUrl = imageUrl ?? "",
                CreatedAt = DateTime.UtcNow,
            };

            await _repo.AddContributionAsync(contribution);
            return true;
        }

        public async Task<bool> SealCapsuleAsync(int capsuleId, DateTime unlockDate, string userId)
        {
            if (unlockDate <= DateTime.UtcNow)
                return false;

            return await _repo.SealCapsuleAsync(capsuleId, unlockDate, userId);
        }

        public async Task<TimeCapsule> GetCapsuleDetailsAsync(int capsuleId, string userId)
        {
            var capsule = await _repo.GetCapsuleWithContributionsAsync(capsuleId, userId);
            if (capsule == null || !capsule.UnlockDate.HasValue || capsule.UnlockDate > DateTime.UtcNow)
            {
                return null;
            }
            var isCreator = capsule.UserId == userId;
            var isContributor = capsule.Contributors.Any(c => c.UserId == userId);

            if (!isCreator && !isContributor)
            {
                return null;
            }

            return capsule;
        }
    }
}