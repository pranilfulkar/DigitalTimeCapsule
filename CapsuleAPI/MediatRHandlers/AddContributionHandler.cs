using MediatR;
using CapsuleAPI.DTO;
using CapsuleAPI.Interface;

namespace CapsuleAPI.MediatRHandlers
{
    public class AddContributionCommand : IRequest<bool>
    {
        public ContributionDto ContributionDto { get; }
        public string UserId { get; }
        public string? ImageUrl { get; }

        public AddContributionCommand(ContributionDto contributionDto, string userId, string? imageUrl)
        {
            ContributionDto = contributionDto;
            UserId = userId;
            ImageUrl = imageUrl;
        }
    }

    public class AddContributionHandler : IRequestHandler<AddContributionCommand, bool>
    {
        private readonly ICapsuleService _service;

        public AddContributionHandler(ICapsuleService service)
        {
            _service = service;
        }

        public async Task<bool> Handle(AddContributionCommand request, CancellationToken cancellationToken)
        {
            return await _service.AddContributionAsync(request.ContributionDto, request.UserId, request.ImageUrl);
        }
    }
}
