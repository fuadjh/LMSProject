using Application.Common.Results;
using MediatR;

namespace Application.Security.Queries.GetPermissionCatalog;

public sealed record GetPermissionCatalogQuery
    : IRequest<Result<IReadOnlyCollection<string>>>;