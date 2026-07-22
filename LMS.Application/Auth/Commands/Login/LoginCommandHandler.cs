using Application.Abstractions.Auth;
using Application.Common.Results;
using Common.Contracts.Auth;

using MediatR;

namespace LMS.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<SignInDataDto>>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<SignInDataDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.ValidateCredentialsAsync(
            request.UserName,
            request.Password,
            cancellationToken);

        if (!userId.HasValue)
        {
            return Result<SignInDataDto>.Unauthorized(
                "auth.invalid_credentials",
                "نام کاربری یا کلمه عبور نادرست است.");
        }

        var signInData = await _identityService.GetSignInDataAsync(userId.Value, cancellationToken);

        if (signInData is null)
        {
            return Result<SignInDataDto>.Unauthorized(
                "auth.user_inactive_or_invalid",
                "کاربر غیرفعال است یا اطلاعات ورود ناقص است.");
        }

        return Result<SignInDataDto>.Success(signInData);
    }
}