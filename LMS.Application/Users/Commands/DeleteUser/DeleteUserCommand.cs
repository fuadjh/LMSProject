using Application.Abstractions.Identity;
using Application.Common.Results;
using MediatR;

namespace Application.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId)
    : IRequest<Result<bool>>;

public sealed class DeleteUserCommandHandler(
    IUserAccountService userAccountService)
    : IRequestHandler<DeleteUserCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
            return Result<bool>.Failure("شناسه کاربر معتبر نیست.");

        var result = await userAccountService.DeleteAsync(
            request.UserId,
            cancellationToken);

        return result.Succeeded
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(result.Errors);
    }
}