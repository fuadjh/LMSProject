using System.Net.Http.Json;
using WebUi.Components.Shared.Crud;


namespace WebUi.Components.Pages.Admin.Academic.Courses;

public sealed class CourseApiClient(HttpClient httpClient)
    : ICrudPageService<CourseListItemVm, CourseFormModel, Guid>
{
    public async Task<IReadOnlyList<CourseListItemVm>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<IReadOnlyList<CourseListItemVm>>("api/courses", cancellationToken);
        return result ?? Array.Empty<CourseListItemVm>();
    }

    public async Task<CourseFormModel> CreateModelAsync(CancellationToken cancellationToken = default)
    {
        return new CourseFormModel
        {
            Units = 3,
            Majors = await GetMajorsAsync(cancellationToken)
        };
    }

    public async Task<CourseFormModel> GetForEditAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await httpClient.GetFromJsonAsync<CourseFormModel>($"api/courses/{id}", cancellationToken)
                    ?? throw new InvalidOperationException("اطلاعات درس پیدا نشد.");

        model.Majors = await GetMajorsAsync(cancellationToken);
        return model;
    }

    public async Task SaveAsync(CourseFormModel model, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;

        if (model.Id.HasValue)
        {
            response = await httpClient.PutAsJsonAsync(
                $"api/courses/{model.Id.Value}",
                new
                {
                    model.Title,
                    model.Code,
                    model.Units
                },
                cancellationToken);
        }
        else
        {
            if (!model.MajorId.HasValue)
                throw new InvalidOperationException("رشته الزامی است.");

            response = await httpClient.PostAsJsonAsync(
                "api/courses",
                new
                {
                    MajorId = model.MajorId.Value,
                    model.Title,
                    model.Code,
                    model.Units
                },
                cancellationToken);
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/courses/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PatchAsJsonAsync(
            $"api/courses/{id}/active",
            new { IsActive = isActive },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private async Task<IReadOnlyList<LookupItemVm>> GetMajorsAsync(CancellationToken cancellationToken)
    {
        var result = await httpClient.GetFromJsonAsync<IReadOnlyList<LookupItemVm>>("api/reference-data/majors", cancellationToken);
        return result ?? Array.Empty<LookupItemVm>();
    }
}