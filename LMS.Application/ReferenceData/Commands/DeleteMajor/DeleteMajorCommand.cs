using Application.Common.Results;
using MediatR;

namespace Application.ReferenceData.Commands.DeleteMajor;

public sealed record DeleteMajorCommand(Guid Id) : IRequest<Result>;
