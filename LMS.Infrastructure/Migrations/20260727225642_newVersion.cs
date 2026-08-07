using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class newVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CourseOfferings_CourseId_SemesterId",
                table: "CourseOfferings");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAtUtc",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnrollmentScope",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "CourseOfferings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndsAtUtc",
                table: "CourseOfferings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SectionCode",
                table: "CourseOfferings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartsAtUtc",
                table: "CourseOfferings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "ExpertProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpertProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpertProfiles_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningItemProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstOpenedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastOpenedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    LastPositionSeconds = table.Column<int>(type: "int", nullable: false),
                    WatchedSeconds = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningItemProgresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    HtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StorageKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HlsMasterPlaylistKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CoverStorageKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    PrerequisiteModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AvailableFromUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AvailableUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningModules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningTemplateItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningTemplateModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    HtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StorageKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HlsMasterPlaylistKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CoverStorageKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTemplateItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningTemplateModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    PrerequisiteTemplateModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AvailableFromOffsetMinutes = table.Column<int>(type: "int", nullable: true),
                    AvailableUntilOffsetMinutes = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTemplateModules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstructorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsShared = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferings_CourseId_SemesterId_SectionCode",
                table: "CourseOfferings",
                columns: new[] { "CourseId", "SemesterId", "SectionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpertProfiles_EmployeeCode",
                table: "ExpertProfiles",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpertProfiles_UserId",
                table: "ExpertProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningItemProgresses_LearningItemId_StudentProfileId",
                table: "LearningItemProgresses",
                columns: new[] { "LearningItemId", "StudentProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningItems_LearningModuleId_Order",
                table: "LearningItems",
                columns: new[] { "LearningModuleId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningModules_CourseOfferingId_Order",
                table: "LearningModules",
                columns: new[] { "CourseOfferingId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningTemplateItems_LearningTemplateModuleId_Order",
                table: "LearningTemplateItems",
                columns: new[] { "LearningTemplateModuleId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningTemplateModules_LearningTemplateId_Order",
                table: "LearningTemplateModules",
                columns: new[] { "LearningTemplateId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningTemplates_CourseId_InstructorProfileId_Title",
                table: "LearningTemplates",
                columns: new[] { "CourseId", "InstructorProfileId", "Title" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpertProfiles");

            migrationBuilder.DropTable(
                name: "LearningItemProgresses");

            migrationBuilder.DropTable(
                name: "LearningItems");

            migrationBuilder.DropTable(
                name: "LearningModules");

            migrationBuilder.DropTable(
                name: "LearningTemplateItems");

            migrationBuilder.DropTable(
                name: "LearningTemplateModules");

            migrationBuilder.DropTable(
                name: "LearningTemplates");

            migrationBuilder.DropIndex(
                name: "IX_CourseOfferings_CourseId_SemesterId_SectionCode",
                table: "CourseOfferings");

            migrationBuilder.DropColumn(
                name: "DeactivatedAtUtc",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "EnrollmentScope",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "CourseOfferings");

            migrationBuilder.DropColumn(
                name: "EndsAtUtc",
                table: "CourseOfferings");

            migrationBuilder.DropColumn(
                name: "SectionCode",
                table: "CourseOfferings");

            migrationBuilder.DropColumn(
                name: "StartsAtUtc",
                table: "CourseOfferings");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferings_CourseId_SemesterId",
                table: "CourseOfferings",
                columns: new[] { "CourseId", "SemesterId" },
                unique: true);
        }
    }
}
