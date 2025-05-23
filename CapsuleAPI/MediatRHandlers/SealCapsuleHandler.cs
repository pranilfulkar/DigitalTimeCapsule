using MediatR;
using CapsuleAPI.Interface;

namespace CapsuleAPI.MediatRHandlers
{
    public class SealCapsuleCommand : IRequest<bool>
    {
        public int CapsuleId { get; }
        public DateTime UnlockDate { get; }
        public string UserId { get; }

        public SealCapsuleCommand(int capsuleId, DateTime unlockDate, string userId)
        {
            CapsuleId = capsuleId;
            UnlockDate = unlockDate;
            UserId = userId;
        }
    }

    public class SealCapsuleHandler : IRequestHandler<SealCapsuleCommand, bool>
    {
        private readonly ICapsuleService _service;

        public SealCapsuleHandler(ICapsuleService service)
        {
            _service = service;
        }

        public async Task<bool> Handle(SealCapsuleCommand request, CancellationToken cancellationToken)
        {
            return await _service.SealCapsuleAsync(request.CapsuleId, request.UnlockDate, request.UserId);
        }
    }
}
