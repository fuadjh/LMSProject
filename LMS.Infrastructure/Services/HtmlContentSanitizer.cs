using Application.Abstractions.Learning;
using Ganss.Xss;

namespace Infrastructure.Services;

public sealed class HtmlContentSanitizer
    : IHtmlContentSanitizer
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlContentSanitizer()
    {
        _sanitizer = new HtmlSanitizer();

        _sanitizer.AllowedAttributes.Add("class");
        _sanitizer.AllowedAttributes.Add("dir");
        _sanitizer.AllowedAttributes.Add("lang");
        _sanitizer.AllowedAttributes.Add("style");

        _sanitizer.AllowedCssProperties.Add("direction");
        _sanitizer.AllowedCssProperties.Add("text-align");
        _sanitizer.AllowedCssProperties.Add("font-family");
        _sanitizer.AllowedCssProperties.Add("font-size");
        _sanitizer.AllowedCssProperties.Add("color");
        _sanitizer.AllowedCssProperties.Add("background-color");

        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("http");
    }

    public string Sanitize(string html) =>
        _sanitizer.Sanitize(html ?? string.Empty);
}