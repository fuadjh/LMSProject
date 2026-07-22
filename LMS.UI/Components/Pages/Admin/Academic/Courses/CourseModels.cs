using System.ComponentModel.DataAnnotations;

namespace WebUi.Components.Pages.Admin.Academic.Courses;

public sealed class CourseListItemVm
{
    public Guid Id { get; set; }
    public string MajorTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Units { get; set; }
    public bool IsActive { get; set; }
}

public sealed class LookupItemVm
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

public sealed class CourseFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "انتخاب رشته الزامی است.")]
    public Guid? MajorId { get; set; }

    [Required(ErrorMessage = "عنوان الزامی است.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "کد درس الزامی است.")]
    public string Code { get; set; } = string.Empty;

    [Range(1, 30, ErrorMessage = "تعداد واحد باید بین 1 تا 30 باشد.")]
    public int Units { get; set; } = 3;

    public IReadOnlyList<LookupItemVm> Majors { get; set; } = Array.Empty<LookupItemVm>();
}