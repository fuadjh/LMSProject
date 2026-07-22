using Application.Abstractions.Read;
using Application.Common.Models;
using Application.Common.Results;
using MediatR;

namespace Application.Security.Queries.GetRoles;

public sealed class GetRolesQueryHandler
    : IRequestHandler<GetRolesQuery, Result<IReadOnlyCollection<RoleListItemDto>>>
{
    private readonly ISecurityReadService _securityReadService;

    public GetRolesQueryHandler(ISecurityReadService securityReadService)
    {
        _securityReadService = securityReadService;
    }

    public async Task<Result<IReadOnlyCollection<RoleListItemDto>>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await _securityReadService.GetRolesAsync(cancellationToken);
        return Result<IReadOnlyCollection<RoleListItemDto>>.Success(roles);
    }
}