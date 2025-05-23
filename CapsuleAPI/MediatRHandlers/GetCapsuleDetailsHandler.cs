using MediatR;
using CapsuleAPI.Model;
using CapsuleAPI.Interface;

namespace CapsuleAPI.MediatRHandlers
{
    public class GetCapsuleDetailsQuery : IRequest<TimeCapsule>
    {
        public int CapsuleId { get; }
        public string UserId { get; }

        public GetCapsuleDetailsQuery(int capsuleId, string userId)
        {
            CapsuleId = capsuleId;
            UserId = userId;
        }
    }

    public class GetCapsuleDetailsHandler : IRequestHandler<GetCapsuleDetailsQuery, TimeCapsule>
    {
        private readonly ICapsuleService _service;

        public GetCapsuleDetailsHandler(ICapsuleService service)
        {
            _service = service;
        }

        public async Task<TimeCapsule> Handle(GetCapsuleDetailsQuery request, CancellationToken cancellationToken)
        {
            return await _service.GetCapsuleDetailsAsync(request.CapsuleId, request.UserId);
        }
    }
}
