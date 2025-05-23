using MediatR;
using CapsuleAPI.DTO;
using CapsuleAPI.Interface;

namespace CapsuleAPI.MediatRHandlers
{
    public class InviteContributorCommand : IRequest<bool>
    {
        public InviteContributorDTO InviteDto { get; }
        public string CreatorUserId { get; }

        public InviteContributorCommand(InviteContributorDTO inviteDto, string creatorUserId)
        {
            InviteDto = inviteDto;
            CreatorUserId = creatorUserId;
        }
    }

    public class InviteContributorHandler : IRequestHandler<InviteContributorCommand, bool>
    {
        private readonly ICapsuleService _service;

        public InviteContributorHandler(ICapsuleService service)
        {
            _service = service;
        }

        public async Task<bool> Handle(InviteContributorCommand request, CancellationToken cancellationToken)
        {
            return await _service.InviteContributorAsync(request.InviteDto, request.CreatorUserId);
        }
    }
}
