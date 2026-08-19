namespace Common.Security;

public static class Permissions
{
    public static class ReferenceData
    {
        public const string View = "ReferenceData.View";
        public const string Manage = "ReferenceData.Manage";
    }

    public static class Courses
    {
        public const string View = "Courses.View";
        public const string Manage = "Courses.Manage";
    }

    public static class Semesters
    {
        public const string View = "Semesters.View";
        public const string Manage = "Semesters.Manage";
    }

    public static class CourseOfferings
    {
        public const string Create = "CourseOfferings.Create";
        public const string View = "CourseOfferings.View";
        public const string Manage = "CourseOfferings.Manage";
        public const string AssignInstructor = "CourseOfferings.AssignInstructor";
        public const string EnrollStudent = "CourseOfferings.EnrollStudent";
        public const string ManageEnrollment = "CourseOfferings.ManageEnrollment";
    }

    public static class Learning
    {
        public const string View = "Learning.View";
        public const string Manage = "Learning.Manage";
    }

    public static class QuestionBank
    {
        public const string View = "QuestionBank.View";
        public const string Manage = "QuestionBank.Manage";
    }

    public static class Exams
    {
        public const string View = "Exams.View";
        public const string Manage = "Exams.Manage";
        public const string Take = "Exams.Take";
        public const string Grade = "Exams.Grade";
    }

    public static class Students
    {
        public const string Create = "Students.Create";
        public const string Edit = "Students.Edit";
        public const string View = "Students.View";
    }

    public static class Instructors
    {
        public const string Create = "Instructors.Create";
        public const string Edit = "Instructors.Edit";
        public const string View = "Instructors.View";
    }

    public static class Experts
    {
        public const string Create = "Experts.Create";
        public const string Edit = "Experts.Edit";
        public const string View = "Experts.View";
    }

    public static class Security
    {
        public const string RolesManage = "Security.Roles.Manage";
        public const string UserRolesAssign = "Security.UserRoles.Assign";
        public const string ScopesAssign = "Security.Scopes.Assign";
        public const string UsersRead = "Security.Users.Read";
        public const string UsersDelete = "Security.Users.Delete";
    }

    public static IReadOnlyList<string> All =>
    [
        ReferenceData.View,
        ReferenceData.Manage,

        Courses.View,
        Courses.Manage,

        Semesters.View,
        Semesters.Manage,

        CourseOfferings.Create,
        CourseOfferings.View,
        CourseOfferings.Manage,
        CourseOfferings.AssignInstructor,
        CourseOfferings.EnrollStudent,
        CourseOfferings.ManageEnrollment,

        Learning.View,
        Learning.Manage,

        QuestionBank.View,
        QuestionBank.Manage,

        Exams.View,
        Exams.Manage,
        Exams.Take,
        Exams.Grade,

        Students.Create,
        Students.Edit,
        Students.View,

        Instructors.Create,
        Instructors.Edit,
        Instructors.View,

        Experts.Create,
        Experts.Edit,
        Experts.View,

        Security.RolesManage,
        Security.UserRolesAssign,
        Security.ScopesAssign,
        Security.UsersRead,
        Security.UsersDelete
    ];
}