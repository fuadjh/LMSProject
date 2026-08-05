using Application.Common.Models;
using Application.Common.Results;
using MediatR;

namespace Application.Users.Queries.GetUserProfileDetails;

public sealed record GetUserProfileDetailsQuery(
    Guid UserProfileId,
    UserProfileType ProfileType)
    : IRequest<Result<UserProfileDetailsDto>>;