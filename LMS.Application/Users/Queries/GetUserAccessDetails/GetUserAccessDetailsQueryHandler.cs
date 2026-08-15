using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using MediatR;

namespace Application.Users.Queries.GetUserAccessDetails;

public sealed class GetUserAccessDetailsQueryHandler
    : IRequestHandler<
        GetUserAccessDetailsQuery,
        Result<UserAccessDetailsDto>>
{
    private readonly IUserAdminReadService _readService;

    public GetUserAccessDetailsQueryHandler(
        IUserAdminReadService readService)
    {
        _readService = readService;
    }

    public async Task<Result<UserAccessDetailsDto>> Handle(
        GetUserAccessDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _readService
            .GetUserAccessDetailsAsync(
                request.UserId,
                cancellationToken);

        return result is null
            ? Result<UserAccessDetailsDto>.NotFound(
                "User.NotFound",
                "کاربر پیدا نشد.")
            : Result<UserAccessDetailsDto>.Success(result);
    }
}