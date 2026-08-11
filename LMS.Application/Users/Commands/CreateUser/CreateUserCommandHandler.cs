using Application.Abstractions.Identity;
using Application.Abstractions.Persistence;
using Application.Common.Models;
using Application.Common.Results;
using Common.Enums;
using Common.Security;
using Domain.Entities.Users;
using MediatR;

namespace Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    IApplicationDbContext dbContext,
    IUserAccountService userAccountService)
    : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        if (await userAccountService.UserNameExistsAsync(
                request.UserName,
                cancellationToken: cancellationToken))
        {
            return Result<Guid>.Failure(
                "نام کاربری قبلاً ثبت شده است.");
        }

        if (await userAccountService.PhoneNumberExistsAsync(
                request.PhoneNumber,
                cancellationToken: cancellationToken))
        {
            return Result<Guid>.Failure(
                "شماره همراه قبلاً ثبت شده است.");
        }

        if (await userAccountService.FindByNationalCodeAsync(
                request.NationalCode,
                cancellationToken) is not null)
        {
            return Result<Guid>.Failure(
                "کد ملی قبلاً ثبت شده است.");
        }

        await using var transaction =
            await dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var identityResult = await userAccountService.CreateAsync(
                new UserIdentityData(
                    request.UserName,
                    request.FirstName,
                    request.LastName,
                    request.NationalCode,
                    request.PhoneNumber,
                    request.Email,
                    request.LatinFirstName,
                    request.LatinLastName,
                    request.Gender,
                    request.ProfileImagePath),
                request.Password,
                cancellationToken);

            if (!identityResult.Succeeded ||
                identityResult.UserId is null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result<Guid>.Failure(identityResult.Errors);
            }

            var userId = identityResult.UserId.Value;

            var userProfile = UserProfile.Create(
                userId,
                request.FirstName,
                request.LastName);

            userProfile.SetFacultyScopes(request.FacultyScopeIds);
            userProfile.SetMajorScopes(request.MajorScopeIds);

            await dbContext.AddAsync(userProfile, cancellationToken);

            switch (request.Role)
            {
                case UserRoleType.Student:
                    await dbContext.AddAsync(
                        StudentProfile.Create(
                            userProfile.Id,
                            request.StudentNumber!,
                            request.StudentMajorId!.Value),
                        cancellationToken);
                    break;

                case UserRoleType.Instructor:
                    await dbContext.AddAsync(
                        InstructorProfile.Create(
                            userProfile.Id,
                            request.PersonnelCode!),
                        cancellationToken);
                    break;

                case UserRoleType.EducationExpert:
                    await dbContext.AddAsync(
                        ExpertProfile.Create(userProfile.Id),
                        cancellationToken);
                    break;

                default:
                    await transaction.RollbackAsync(cancellationToken);

                    return Result<Guid>.Failure(
                        "نوع کاربر معتبر نیست.");
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            var roleResult = await userAccountService.AddToRoleAsync(
                userId,
                GetRoleName(request.Role),
                cancellationToken);

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result<Guid>.Failure(roleResult.Errors);
            }

            await transaction.CommitAsync(cancellationToken);

            return Result<Guid>.Success(userId);
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            return Result<Guid>.Failure(
                $"ایجاد کاربر ناموفق بود: {exception.Message}");
        }
    }

    private static string GetRoleName(UserRoleType role) =>
        role switch
        {
            UserRoleType.Student => RoleNames.Student,
            UserRoleType.Instructor => RoleNames.Instructor,
            UserRoleType.EducationExpert => RoleNames.EducationExpert,
            _ => throw new ArgumentOutOfRangeException(nameof(role))
        };
}