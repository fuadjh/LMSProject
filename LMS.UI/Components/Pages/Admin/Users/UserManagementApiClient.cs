using System.Net;
using System.Net.Http.Json;
using WebUi.Services;

namespace WebUi.Components.Pages.Admin.Users;

public sealed class UserManagementApiClient
{
    private readonly HttpClient _httpClient;

    public UserManagementApiClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResponseVm<UserProfileListItemVm>> GetUsersAsync(
        UserProfileTypeVm profileType,
        int pageNumber,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var route = GetRoute(profileType);

        var url =
            $"api/{route}?pageNumber={pageNumber}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(search))
        {
            url +=
                $"&search={Uri.EscapeDataString(search.Trim())}";
        }

        return await _httpClient.GetFromJsonAsync<
                   PagedResponseVm<UserProfileListItemVm>>(
                   url,
                   cancellationToken)
               ?? new PagedResponseVm<UserProfileListItemVm>();
    }

    public async Task<UserProfileDetailsVm> GetDetailsAsync(
        UserProfileTypeVm profileType,
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        var route = GetRoute(profileType);

        return await _httpClient.GetFromJsonAsync<UserProfileDetailsVm>(
                   $"api/{route}/{userProfileId}",
                   cancellationToken)
               ?? throw new InvalidOperationException(
                   "اطلاعات کاربر یافت نشد.");
    }

    public async Task UpdateAsync(
        UserProfileTypeVm profileType,
        UserEditModel model,
        CancellationToken cancellationToken = default)
    {
        var route = GetRoute(profileType);

        var response = await _httpClient.PutAsJsonAsync(
            $"api/{route}/{model.UserProfileId}",
            new
            {
                model.FirstName,
                model.LastName,
                model.Email
            },
            cancellationToken);

        await EnsureSuccessAsync(response);
    }

    public async Task SetActiveStatusAsync(
        UserProfileTypeVm profileType,
        Guid userProfileId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var route = GetRoute(profileType);

        var response = await _httpClient.PatchAsJsonAsync(
            $"api/{route}/{userProfileId}/active",
            new
            {
                IsActive = isActive
            },
            cancellationToken);

        await EnsureSuccessAsync(response);
    }

    public async Task DeleteAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/users/{userProfileId}",
            cancellationToken);

        await EnsureSuccessAsync(response);
    }

    public async Task<ExistingPersonVm?> GetByNationalCodeAsync(
        string nationalCode,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"api/users/by-national-code/{Uri.EscapeDataString(nationalCode)}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response);

        return await response.Content
            .ReadFromJsonAsync<ExistingPersonVm>(
                cancellationToken: cancellationToken);
    }

    public async Task<UserAccessDetailsVm> GetAccessAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<UserAccessDetailsVm>(
                   $"api/users/{userProfileId}/access",
                   cancellationToken)
               ?? throw new InvalidOperationException(
                   "اطلاعات دسترسی کاربر یافت نشد.");
    }

    public async Task<IReadOnlyCollection<RoleVm>> GetRolesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<
                   IReadOnlyCollection<RoleVm>>(
                   "api/users/roles",
                   cancellationToken)
               ?? Array.Empty<RoleVm>();
    }

    public async Task<IReadOnlyCollection<LookupVm>> GetFacultiesAsync(
        CancellationToken cancellationToken = default)
    {
        var response =
            await _httpClient.GetFromJsonAsync<
                PagedResponseVm<LookupVm>>(
                "api/reference-data/faculties?pageNumber=1&pageSize=100",
                cancellationToken);

        return response?.Items
               ?? Array.Empty<LookupVm>();
    }

    public async Task<IReadOnlyCollection<LookupVm>> GetMajorsAsync(
        Guid facultyId,
        CancellationToken cancellationToken = default)
    {
        var response =
            await _httpClient.GetFromJsonAsync<
                PagedResponseVm<LookupVm>>(
                $"api/reference-data/majors?facultyId={facultyId}&pageNumber=1&pageSize=100",
                cancellationToken);

        return response?.Items
               ?? Array.Empty<LookupVm>();
    }

    public async Task SaveAccessAsync(
        UserAccessDetailsVm model,
        CancellationToken cancellationToken = default)
    {
        var roleResponse = await _httpClient.PutAsJsonAsync(
            $"api/user-access/{model.AuthUserId}/roles",
            new
            {
                model.Roles
            },
            cancellationToken);

        await EnsureSuccessAsync(roleResponse);

        var scopeResponse = await _httpClient.PutAsJsonAsync(
            $"api/user-access/profiles/{model.UserProfileId}/scopes",
            new
            {
                model.FacultyIds,
                model.MajorIds
            },
            cancellationToken);

        await EnsureSuccessAsync(scopeResponse);
    }

    public async Task CreateStudentAsync(
        CreateStudentVm model,
        CancellationToken cancellationToken = default)
    {
        if (!model.MajorId.HasValue)
            throw new InvalidOperationException(
                "انتخاب رشته الزامی است.");

        var response = await _httpClient.PostAsJsonAsync(
            "api/students",
            new
            {
                model.NationalCode,
                model.UserName,
                model.Email,
                model.Password,
                model.FirstName,
                model.LastName,
                model.StudentNumber,
                MajorId = model.MajorId.Value
            },
            cancellationToken);

        await EnsureSuccessAsync(response);
    }

    public async Task CreateInstructorAsync(
        CreateInstructorVm model,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/instructors",
            model,
            cancellationToken);

        await EnsureSuccessAsync(response);
    }

    public async Task CreateEducationExpertAsync(
        CreateEducationExpertVm model,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/education-experts",
            model,
            cancellationToken);

        await EnsureSuccessAsync(response);
    }

    private static string GetRoute(
        UserProfileTypeVm profileType)
    {
        return profileType switch
        {
            UserProfileTypeVm.Student =>
                "students",

            UserProfileTypeVm.Instructor =>
                "instructors",

            UserProfileTypeVm.EducationExpert =>
                "education-experts",

            _ => throw new ArgumentOutOfRangeException(
                nameof(profileType))
        };
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        throw new InvalidOperationException(
            await ApiErrorReader.ReadErrorAsync(response));
    }
}