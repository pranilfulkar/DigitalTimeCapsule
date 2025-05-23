using MediatR;
using CapsuleAPI.DTO;
using CapsuleAPI.Interface;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapsuleAPI.MediatRHandlers
{
    public record GetUserCapsulesQuery(
        string UserId,
        string SortBy = "unlockDate",
        string SortDirection = "desc",
        int PageIndex = 0,
        int PageSize = 10,
        string? Search = null
    ) : IRequest<(List<CapsuleListViewDto> capsules, int totalCount)>;

    public class GetUserCapsulesHandler : IRequestHandler<GetUserCapsulesQuery, (List<CapsuleListViewDto>, int)>
    {
        private readonly ICapsuleService _service;

        public GetUserCapsulesHandler(ICapsuleService service)
        {
            _service = service;
        }

        public async Task<(List<CapsuleListViewDto>, int)> Handle(GetUserCapsulesQuery request, CancellationToken cancellationToken)
        {
            return await _service.GetUserCapsulesAsync(request.UserId, request.SortBy, request.SortDirection, request.PageIndex, request.PageSize, request.Search);
        }
    }
}