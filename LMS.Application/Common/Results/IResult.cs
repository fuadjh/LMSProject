namespace Application.Common.Results;

public interface IResult
{
    bool IsSuccess { get; }
    IReadOnlyCollection<Error> Errors { get; }
}