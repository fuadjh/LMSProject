namespace Application.Abstractions.Learning;

public interface IHtmlContentSanitizer
{
    string Sanitize(string html);
}