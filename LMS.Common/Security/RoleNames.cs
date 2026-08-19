namespace Common.Security;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Expert = "Expert";
    public const string Instructor = "Instructor";
    public const string Student = "Student";

    public static IReadOnlyCollection<string> All => new[]
    {
        Admin,
        Expert,
        Instructor,
        Student
    };
}