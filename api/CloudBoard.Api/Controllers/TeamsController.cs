using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Services;
using CloudBoard.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Asp.Versioning;

namespace CloudBoard.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim!);
    }

    /// <summary>
    /// Gets all teams the current user is a member of
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TeamDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeams()
    {
        var userId = GetCurrentUserId();
        var result = await _teamService.GetUserTeamsAsync(userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets a specific team by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TeamDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTeam(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _teamService.GetTeamByIdAsync(id, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Creates a new team
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTeam(CreateTeamDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _teamService.CreateTeamAsync(dto, userId);
        return result.ToCreatedResult(this, nameof(GetTeam), new { id = result.Value?.Id });
    }

    /// <summary>
    /// Updates a team's settings
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTeam(int id, UpdateTeamDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _teamService.UpdateTeamAsync(id, dto, userId);

        if (result.IsSuccess)
            return NoContent();

        return result.ToActionResult();
    }

    /// <summary>
    /// Deletes a team (owner only, must have no projects)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _teamService.DeleteTeamAsync(id, userId);

        if (result.IsSuccess)
            return NoContent();

        return result.ToActionResult();
    }

    // === Member Management ===

    /// <summary>
    /// Invites a user to the team
    /// </summary>
    [HttpPost("{id}/invitations")]
    [ProducesResponseType(typeof(TeamInvitationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> InviteMember(int id, InviteMemberDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _teamService.InviteMemberAsync(id, dto, userId);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetTeam), new { id }, result.Value);

        return result.ToActionResult();
    }

    /// <summary>
    /// Cancels a pending invitation
    /// </summary>
    [HttpDelete("{teamId}/invitations/{invitationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelInvitation(int teamId, int invitationId)
    {
        var userId = GetCurrentUserId();
        var result = await _teamService.CancelInvitationAsync(teamId, invitationId, userId);

        if (result.IsSuccess)
            return NoContent();

        return result.ToActionResult();
    }

    /// <summary>
    /// Updates a member's role
    /// </summary>
    [HttpPut("{teamId}/members/{memberId}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateMemberRole(int teamId, int memberId, UpdateMemberRoleDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _teamService.UpdateMemberRoleAsync(teamId, memberId, dto, userId);

        if (result.IsSuccess)
            return NoContent();

        return result.ToActionResult();
    }

    /// <summary>
    /// Removes a member from the team
    /// </summary>
    [HttpDelete("{teamId}/members/{memberId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveMember(int teamId, int memberId)
    {
        var userId = GetCurrentUserId();
        var result = await _teamService.RemoveMemberAsync(teamId, memberId, userId);

        if (result.IsSuccess)
            return NoContent();

        return result.ToActionResult();
    }

    /// <summary>
    /// Leave the team (current user)
    /// </summary>
    [HttpPost("{id}/leave")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LeaveTeam(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _teamService.LeaveTeamAsync(id, userId);

        if (result.IsSuccess)
            return NoContent();

        return result.ToActionResult();
    }
}
