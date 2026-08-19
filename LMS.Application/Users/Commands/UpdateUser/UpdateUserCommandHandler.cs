using Application.Abstractions.Identity;
using Application.Abstractions.Persistence;
using Application.Common.Results;
using Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler(
    IApplicationDbContext dbContext,
    IUserAccountService userAccountService)
    : IRequestHandler<UpdateUserCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var userProfile = await dbContext.UserProfiles
            .SingleOrDefaultAsync(
                x => x.AuthUserId == request.UserId,
                cancellationToken);

        if (userProfile is null)
            return Result<bool>.Failure("پروفایل کاربر پیدا نشد.");

        if (await userAccountService.PhoneNumberExistsAsync(
                request.PhoneNumber,
                request.UserId,
                cancellationToken))
        {
            return Result<bool>.Failure(
                "شماره همراه برای کاربر دیگری ثبت شده است.");
        }

        await using var transaction =
            await dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var identityResult = await userAccountService.UpdateAsync(
                request.UserId,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.Email,
                request.LatinFirstName,
                request.LatinLastName,
                request.Gender,
                request.ProfileImagePath,
                cancellationToken);

            if (!identityResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<bool>.Failure(identityResult.Errors);
            }

            var statusResult = await userAccountService.SetActiveAsync(
                request.UserId,
                request.IsActive,
                cancellationToken);

            if (!statusResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<bool>.Failure(statusResult.Errors);
            }

            userProfile.UpdateName(
                request.FirstName,
                request.LastName);

            userProfile.Activate();
           

            switch (request.Role)
            {
                case UserRoleType.Student:
                    {
                        var student = await dbContext.StudentProfiles
                            .SingleOrDefaultAsync(
                                x => x.UserProfileId == userProfile.Id,
                                cancellationToken);

                        if (student is null)
                            return Result<bool>.Failure(
                                "پروفایل دانشجو پیدا نشد.");

                        student.Update(
                            request.StudentNumber!,
                            request.StudentMajorId!.Value);

                        break;
                    }

                case UserRoleType.Instructor:
                    {
                        var instructor = await dbContext.InstructorProfiles
                            .SingleOrDefaultAsync(
                                x => x.UserProfileId == userProfile.Id,
                                cancellationToken);

                        if (instructor is null)
                            return Result<bool>.Failure(
                                "پروفایل مدرس پیدا نشد.");

                        instructor.Update(request.PersonnelCode!);
                        break;
                    }

                case UserRoleType.Expert:
                    {
                        var exists = await dbContext.ExpertProfiles
                            .AnyAsync(
                                x => x.UserProfileId == userProfile.Id,
                                cancellationToken);

                        if (!exists)
                            return Result<bool>.Failure(
                                "پروفایل کارشناس آموزش پیدا نشد.");

                        break;
                    }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            return Result<bool>.Failure(
                $"ویرایش کاربر ناموفق بود: {exception.Message}");
        }
    }
}