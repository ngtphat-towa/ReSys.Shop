using ReSys.Core.Domain.Identity.Users.Profiles.StaffProfiles;

namespace ReSys.Core.UnitTests.Domain.Identity.Users.Profiles.StaffProfiles;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Domain", "StaffProfile")]
public class StaffProfileTests
{
    private readonly string _userId = Guid.NewGuid().ToString();

    [Fact(DisplayName = "Create: Should successfully initialize staff profile")]
    public void Create_Should_InitializeStaffProfile()
    {
        // Act
        var result = ReSys.Core.Domain.Identity.Users.Profiles.StaffProfiles.StaffProfile.Create(userId: _userId, employeeId: "EMP001");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.UserId.Should().Be(_userId);
        result.Value.EmployeeId.Should().Be("EMP001");
    }

    [Fact(DisplayName = "Create: Should fail if employee id too long")]
    public void Create_ShouldFail_IfEmployeeIdTooLong()
    {
        // Arrange
        var longId = new string('A', StaffProfileConstraints.EmployeeIdMaxLength + 1);

        // Act
        var result = ReSys.Core.Domain.Identity.Users.Profiles.StaffProfiles.StaffProfile.Create(userId: _userId, employeeId: longId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(StaffProfileErrors.EmployeeIdTooLong(StaffProfileConstraints.EmployeeIdMaxLength));
    }

    [Fact(DisplayName = "Update: Should change professional details")]
    public void Update_Should_ChangeDetails()
    {
        // Arrange
        var profile = ReSys.Core.Domain.Identity.Users.Profiles.StaffProfiles.StaffProfile.Create(userId: _userId, employeeId: "EMP001").Value;

        // Act
        var result = profile.Update("Engineering", "Lead Developer", "EMP001-NEW");

        // Assert
        result.IsError.Should().BeFalse();
        profile.Department.Should().Be("Engineering");
        profile.JobTitle.Should().Be("Lead Developer");
        profile.EmployeeId.Should().Be("EMP001-NEW");
    }

    [Fact(DisplayName = "Update: Should fail if department too long")]
    public void Update_ShouldFail_IfDepartmentTooLong()
    {
        // Arrange
        var profile = ReSys.Core.Domain.Identity.Users.Profiles.StaffProfiles.StaffProfile.Create(userId: _userId).Value;
        var longDept = new string('A', StaffProfileConstraints.DepartmentMaxLength + 1);

        // Act
        var result = profile.Update(longDept, "Lead");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(StaffProfileErrors.DepartmentTooLong(StaffProfileConstraints.DepartmentMaxLength));
    }
}
