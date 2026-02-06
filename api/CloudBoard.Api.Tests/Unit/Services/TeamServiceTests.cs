using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Repositories;
using CloudBoard.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CloudBoard.Api.Tests.Unit.Services;

public class TeamServiceTests
{
    private readonly Mock<ITeamRepository> _teamRepoMock;
    private readonly Mock<ILogger<TeamService>> _loggerMock;
    private readonly TeamService _sut;

    public TeamServiceTests()
    {
        _teamRepoMock = new Mock<ITeamRepository>();
        _loggerMock = new Mock<ILogger<TeamService>>();
        _sut = new TeamService(_teamRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateTeam_ShouldAddOwnerMembership()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateTeamDto { Name = "Test Team", Description = "Test" };
        
        _teamRepoMock.Setup(r => r.Add(It.IsAny<Team>()));
        _teamRepoMock.Setup(r => r.AddMember(It.IsAny<TeamMember>()));
        _teamRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1); // Fixed: Returns Task<int>

        // Act
        var result = await _sut.CreateTeamAsync(dto, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentUserRole.Should().Be(TeamRole.Owner);
        _teamRepoMock.Verify(r => r.AddMember(It.Is<TeamMember>(m => 
            m.UserId == userId && m.Role == TeamRole.Owner)), Times.Once);
    }

    [Fact]
    public async Task InviteMember_WhenNotAdmin_ShouldReturnForbidden()
    {
        // Arrange
        var teamId = 1;
        var userId = 2;
        var dto = new InviteMemberDto { Email = "test@test.com", Role = TeamRole.Member };

        var team = new Team
        {
            Id = teamId,
            Name = "Test",
            Members = new List<TeamMember>
            {
                new() { UserId = userId, Role = TeamRole.Member }
            }
        };

        _teamRepoMock.Setup(r => r.GetWithMembersAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        // Act
        var result = await _sut.InviteMemberAsync(teamId, dto, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You must be an admin to invite members");
    }

    [Fact]
    public async Task InviteMember_WhenAdmin_ShouldSucceed()
    {
        // Arrange
        var teamId = 1;
        var adminUserId = 1;
        var dto = new InviteMemberDto { Email = "newuser@test.com", Role = TeamRole.Member };

        var team = new Team
        {
            Id = teamId,
            Name = "Test",
            Members = new List<TeamMember>
            {
                new() { UserId = adminUserId, Role = TeamRole.Admin, User = new User { Email = "admin@test.com", Name = "Admin" } }
            }
        };

        _teamRepoMock.Setup(r => r.GetWithMembersAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
        _teamRepoMock.Setup(r => r.GetPendingInvitationAsync(teamId, dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TeamInvitation?)null);
        _teamRepoMock.Setup(r => r.AddInvitation(It.IsAny<TeamInvitation>()));
        _teamRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1); // Fixed: Returns Task<int>

        // Act
        var result = await _sut.InviteMemberAsync(teamId, dto, adminUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _teamRepoMock.Verify(r => r.AddInvitation(It.IsAny<TeamInvitation>()), Times.Once);
    }

    [Fact]
    public async Task RemoveMember_CannotRemoveOwner()
    {
        // Arrange
        var teamId = 1;
        var adminUserId = 2;
        var ownerUserId = 1;

        var team = new Team
        {
            Id = teamId,
            Name = "Test",
            Members = new List<TeamMember>
            {
                new() { UserId = ownerUserId, Role = TeamRole.Owner },
                new() { UserId = adminUserId, Role = TeamRole.Admin }
            }
        };

        _teamRepoMock.Setup(r => r.GetWithMembersAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        // Act
        var result = await _sut.RemoveMemberAsync(teamId, ownerUserId, adminUserId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cannot remove the team owner");
    }

    [Fact]
    public async Task LeaveTeam_OwnerCannotLeave()
    {
        // Arrange
        var teamId = 1;
        var ownerUserId = 1;

        var membership = new TeamMember { TeamId = teamId, UserId = ownerUserId, Role = TeamRole.Owner };

        _teamRepoMock.Setup(r => r.GetMembershipAsync(teamId, ownerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        // Act
        var result = await _sut.LeaveTeamAsync(teamId, ownerUserId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Team owner cannot leave");
    }

    [Fact]
    public async Task DeleteTeam_OnlyOwnerCanDelete()
    {
        // Arrange
        var teamId = 1;
        var adminUserId = 2;

        _teamRepoMock.Setup(r => r.GetWithMembersAndProjectsAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Team { Id = teamId, Name = "Test", Projects = new List<Project>() });
        _teamRepoMock.Setup(r => r.HasRoleOrHigherAsync(teamId, adminUserId, TeamRole.Owner, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteTeamAsync(teamId, adminUserId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Only the team owner can delete");
    }
}
