using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace WebUi.Services;

public static class ApiErrorReader
{
    public static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var validation = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            if (validation?.Errors?.Any() == true)
                return string.Join(" | ", validation.Errors.SelectMany(x => x.Value));
        }
        catch { }

        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            if (!string.IsNullOrWhiteSpace(problem?.Title))
                return problem.Title;
        }
        catch { }

        return $"خطا: {response.StatusCode}";
    }
}