# بسته جایگزین فاز دوم LMS - نسخه 2

این فایل جایگزین لینک قبلی است.

## اعمال Patch

1. Backup بگیرید.
2. فایل‌های Domain، Common، Infrastructure و WebApi را Replace کنید.
3. قبل از افزودن فایل یکپارچه زیر، کلاس‌های تکراری قدیمی را حذف کنید:

```text
LMS.Application/Phase2/AcademicPhase2Features.cs
```

کلاس‌های تکراری مربوط به Update/Delete/Paged Course، Semester و CourseOffering نباید هم‌زمان در فایل دیگری باقی بمانند.

## Migration

```bash
dotnet ef migrations add AddCourseOfferingSectionsAndCourseScope   --project LMS.Infrastructure   --startup-project LMS.WebApi
```

Defaultهای داده قبلی:

```text
Courses.EnrollmentScope = 0
CourseOfferings.SectionCode = "01"
CourseOfferings.Capacity = 30
```

Index قبلی CourseId+SemesterId حذف و Index زیر ایجاد شود:

```text
CourseId + SemesterId + SectionCode
```

سپس:

```bash
dotnet ef database update   --project LMS.Infrastructure   --startup-project LMS.WebApi

dotnet restore
dotnet build
```

## اصلاح Program

ثبت MediatR، Validators و ValidationBehavior فقط یک بار انجام شود.

## Scope

- Admin دسترسی کامل دارد.
- EducationExpert از UserFacultyScope و UserMajorScope استفاده می‌کند.
- MajorId مالک آموزشی درس است.
- EnrollmentScope محدودیت رشته ثبت‌نام را تعیین می‌کند.
