using Application.Common.Models;
using Application.Common.Results;
using MediatR;

namespace Application.Users.Queries.GetUserAccessDetails;

public sealed record GetUserAccessDetailsQuery(
    Guid UserProfileId) : IRequest<Result<UserAccessDetailsDto>>;