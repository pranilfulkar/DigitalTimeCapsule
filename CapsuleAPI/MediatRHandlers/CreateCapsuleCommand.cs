using MediatR;
using CapsuleAPI.DTO;
using CapsuleAPI.Interface;

namespace CapsuleAPI.MediatRHandlers
{
    public class CreateCapsuleCommand : IRequest<bool>
    {
        public CapsuleDto CapsuleDto { get; }
        public string UserId { get; }

        public CreateCapsuleCommand(CapsuleDto capsuleDto, string userId)
        {
            CapsuleDto = capsuleDto;
            UserId = userId;
        }
    }

    public class CreateCapsuleHandler : IRequestHandler<CreateCapsuleCommand, bool>
    {
        private readonly ICapsuleService _service;

        public CreateCapsuleHandler(ICapsuleService service)
        {
            _service = service;
        }

        public async Task<bool> Handle(CreateCapsuleCommand request, CancellationToken cancellationToken)
        {
            return await _service.CreateCapsuleAsync(request.CapsuleDto, request.UserId);
        }
    }
}
