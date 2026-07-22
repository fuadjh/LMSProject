using Application.Common.Results;
using Common.Contracts.Auth;

using MediatR;

namespace LMS.Application.Auth.Commands.Login;

public sealed record LoginCommand(
    string UserName,
    string Password) : IRequest<Result<SignInDataDto>>;