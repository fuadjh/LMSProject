using Application.Abstractions.Read;
using Application.Common.Results;
using MediatR;

namespace Application.Security.Queries.GetPermissionCatalog;

public sealed class GetPermissionCatalogQueryHandler
    : IRequestHandler<GetPermissionCatalogQuery, Result<IReadOnlyCollection<string>>>
{
    private readonly ISecurityReadService _securityReadService;

    public GetPermissionCatalogQueryHandler(ISecurityReadService securityReadService)
    {
        _securityReadService = securityReadService;
    }

    public async Task<Result<IReadOnlyCollection<string>>> Handle(
        GetPermissionCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await _securityReadService.GetPermissionCatalogAsync(cancellationToken);
        return Result<IReadOnlyCollection<string>>.Success(permissions);
    }
}