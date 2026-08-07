using System.Net.Http.Json;

namespace WebUi.Components.Pages.Admin.Users;

public sealed class UserCreateApiClient
{
    private readonly HttpClient _httpClient;

    public UserCreateApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(
        CreateUserModel model,
        CancellationToken cancellationToken = default)
    {
        var response = model.Type switch
        {
            UserCreateType.Student =>
                await _httpClient.PostAsJsonAsync(
                    "/api/students",
                    new
                    {
                        model.FirstName,
                        model.LastName,
                        model.NationalCode,
                        model.PhoneNumber,
                        model.Email,
                        model.LatinFirstName,
                        model.LatinLastName,
                        Gender = (int)model.Gender,
                        model.ProfileImagePath,
                        model.StudentNumber,
                        MajorId = model.MajorId,
                        model.Password
                    },
                    cancellationToken),

            UserCreateType.Instructor =>
                await _httpClient.PostAsJsonAsync(
                    "/api/instructors",
                    new
                    {
                        model.FirstName,
                        model.LastName,
                        model.NationalCode,
                        model.PhoneNumber,
                        model.Email,
                        model.LatinFirstName,
                        model.LatinLastName,
                        Gender = (int)model.Gender,
                        model.ProfileImagePath,
                        model.PersonnelCode,
                        model.Password
                    },
                    cancellationToken),

            UserCreateType.Expert =>
                await _httpClient.PostAsJsonAsync(
                    "/api/education-experts",
                    new
                    {
                        model.UserName,
                        model.FirstName,
                        model.LastName,
                        model.NationalCode,
                        model.PhoneNumber,
                        model.Email,
                        model.LatinFirstName,
                        model.LatinLastName,
                        Gender = (int)model.Gender,
                        model.ProfileImagePath,
                        model.Password
                    },
                    cancellationToken),

            _ => throw new ArgumentOutOfRangeException()
        };

        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var error =
            await response.Content.ReadFromJsonAsync<ApiErrorResponse>(
                cancellationToken: cancellationToken);

        return (
            false,
            error?.Errors is { Length: > 0 }
                ? string.Join("، ", error.Errors)
                : "ایجاد کاربر ناموفق بود.");
    }
}