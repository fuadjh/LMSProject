using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using Domain.Entities.Users;
using MediatR;

namespace Application.Users.Queries.GetUserByNationalCode;

public sealed class GetUserByNationalCodeQueryHandler
    : IRequestHandler<
        GetUserByNationalCodeQuery,
        Result<UserByNationalCodeDto>>
{
    private readonly IUserAdminReadService _readService;

    public GetUserByNationalCodeQueryHandler(
        IUserAdminReadService readService)
    {
        _readService = readService;
    }

    public async Task<Result<UserByNationalCodeDto>> Handle(
        GetUserByNationalCodeQuery request,
        CancellationToken cancellationToken)
    {
        var nationalCode =
            UserProfile.NormalizeNationalCode(
                request.NationalCode);

        var user =
            await _readService.GetUserByNationalCodeAsync(
                nationalCode,
                cancellationToken);

        if (user is null)
        {
            return Result<UserByNationalCodeDto>.NotFound(
                "user.national_code_not_found",
                "کاربری با این کد ملی یافت نشد.");
        }

        return Result<UserByNationalCodeDto>.Success(user);
    }
}