using Application.Common.Models;
using Application.Common.Results;
using MediatR;

namespace Application.Users.Queries.GetUserByNationalCode;

public sealed record GetUserByNationalCodeQuery(
    string NationalCode)
    : IRequest<Result<UserByNationalCodeDto>>;