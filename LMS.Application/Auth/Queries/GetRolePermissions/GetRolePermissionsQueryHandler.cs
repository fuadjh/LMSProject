using Application.Abstractions.Read;
using Application.Common.Results;

using MediatR;

namespace Application.Security.Queries.GetRolePermissions;

public sealed class GetRolePermissionsQueryHandler
    : IRequestHandler<GetRolePermissionsQuery, Result<IReadOnlyCollection<string>>>
{
    private readonly ISecurityReadService _securityReadService;

    public GetRolePermissionsQueryHandler(ISecurityReadService securityReadService)
    {
        _securityReadService = securityReadService;
    }

    public async Task<Result<IReadOnlyCollection<string>>> Handle(
        GetRolePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await _securityReadService.GetRolePermissionsAsync(
            request.RoleId,
            cancellationToken);

        return Result<IReadOnlyCollection<string>>.Success(permissions);
    }
}