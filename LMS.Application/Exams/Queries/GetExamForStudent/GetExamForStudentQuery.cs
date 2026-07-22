using Application.Common.Results;
using LMS.Application.Exams;
using MediatR;

namespace Application.Exams.Queries.GetExamForStudent;

public sealed record GetExamForStudentQuery(Guid ExamId) : IRequest<Result<ExamForStudentDto>>;