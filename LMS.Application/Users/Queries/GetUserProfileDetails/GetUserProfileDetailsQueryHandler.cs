using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using MediatR;

namespace Application.Users.Queries.GetUserProfileDetails;

public sealed class GetUserProfileDetailsQueryHandler
    : IRequestHandler<
        GetUserProfileDetailsQuery,
        Result<UserProfileDetailsDto>>
{
    private readonly IUserAdminReadService _readService;

    public GetUserProfileDetailsQueryHandler(
        IUserAdminReadService readService)
    {
        _readService = readService;
    }

    public async Task<Result<UserProfileDetailsDto>> Handle(
        GetUserProfileDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var details =
            await _readService.GetUserProfileDetailsAsync(
                request.UserProfileId,
                request.ProfileType,
                cancellationToken);

        if (details is null)
        {
            return Result<UserProfileDetailsDto>.NotFound(
                "user_profile.not_found",
                "پروفایل کاربر یافت نشد.");
        }

        return Result<UserProfileDetailsDto>.Success(
            details);
    }
}