# رابط کاربری فاز دوم

صفحات موجود زیر را با الگوی CRUD سالم University هماهنگ کنید:

- `Components/Pages/Admin/Academic/Courses/CoursesPage.razor`
- `Components/Pages/Admin/Academic/Courses/CourseUpsertDialog.razor`
- `Components/Pages/Admin/Academic/Courses/CourseDeleteDialog.razor`
- `Components/Pages/Admin/Academic/Semesters.razor`
- `Components/Pages/Admin/Academic/SemesterDialog.razor`
- `Components/Pages/Admin/Academic/SemesterDeleteDialog.razor`
- `Components/Pages/Admin/Academic/CourseOfferings.razor`
- `Components/Pages/Admin/Academic/CourseOfferingDialog.razor`
- `Components/Pages/Admin/Academic/CourseOfferingDeleteDialog.razor`
- `Components/Pages/Admin/Academic/OfferingEnrollments.razor`

Endpointهای UI:

```text
GET    api/courses/paged
POST   api/courses
PUT    api/courses/{id}
DELETE api/courses/{id}

GET    api/semesters/paged
POST   api/semesters
PUT    api/semesters/{id}
DELETE api/semesters/{id}

GET    api/course-offerings
POST   api/course-offerings
PUT    api/course-offerings/{id}
DELETE api/course-offerings/{id}
PUT    api/course-offerings/{id}/instructor
DELETE api/course-offerings/{id}/instructor
GET    api/course-offerings/{id}/enrollments
POST   api/course-offerings/{id}/enrollments
DELETE api/course-offerings/enrollments/{enrollmentId}
```

در فرم Course یک `MudSelect<CourseEnrollmentScope>` با دو گزینه زیر اضافه شود:

- `OwningMajorOnly`
- `AllMajors`

در فرم CourseOffering دو فیلد زیر اضافه شود:

- `SectionCode`
- `Capacity`
