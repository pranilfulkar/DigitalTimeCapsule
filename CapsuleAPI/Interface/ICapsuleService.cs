using CapsuleAPI.DTO;
using CapsuleAPI.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CapsuleAPI.Interface
{
    public interface ICapsuleService
    {
        Task<(List<CapsuleListViewDto> capsules, int totalCount)> GetUserCapsulesAsync(string userId, string sortBy = "unlockDate", string sortDirection = "desc", int pageIndex = 0, int pageSize = 10, string? search = null);
        Task<bool> CreateCapsuleAsync(CapsuleDto capsuleDto, string userId);
        Task<bool> InviteContributorAsync(InviteContributorDTO inviteDto, string createrUserId);
        Task<bool> AddContributionAsync(ContributionDto contributionDto, string userId, string? imageUrl);
        Task<bool> SealCapsuleAsync(int capsuleId, DateTime unlockDate, string userId);
        Task<TimeCapsule> GetCapsuleDetailsAsync(int capsuleId, string userId);
    }
}