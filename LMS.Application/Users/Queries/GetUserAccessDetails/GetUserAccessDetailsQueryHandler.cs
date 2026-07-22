using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using MediatR;

namespace Application.Users.Queries.GetUserAccessDetails;

public sealed class GetUserAccessDetailsQueryHandler
    : IRequestHandler<GetUserAccessDetailsQuery, Result<UserAccessDetailsDto>>
{
    private readonly IUserAdminReadService _userAdminReadService;

    public GetUserAccessDetailsQueryHandler(IUserAdminReadService userAdminReadService)
    {
        _userAdminReadService = userAdminReadService;
    }

    public async Task<Result<UserAccessDetailsDto>> Handle(
        GetUserAccessDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var details = await _userAdminReadService.GetUserAccessDetailsAsync(
            request.UserProfileId,
            cancellationToken);

        if (details is null)
        {
            return Result<UserAccessDetailsDto>.NotFound(
                "user_access.not_found",
                "اطلاعات دسترسی کاربر یافت نشد.");
        }

        return Result<UserAccessDetailsDto>.Success(details);
    }
}