using Application.Common.Results;
using MediatR;

namespace Application.Exams.Commands.SubmitExamAttempt;

public sealed record SubmitExamAttemptCommand(
    Guid SubmissionId,
    IReadOnlyCollection<SubmitExamAnswerItem> Answers) : IRequest<Result>;

public sealed record SubmitExamAnswerItem(
    Guid QuestionId,
    Guid? SelectedOptionId,
    string? EssayText,
    string? EssayAttachmentFileName,
    string? EssayAttachmentPath);