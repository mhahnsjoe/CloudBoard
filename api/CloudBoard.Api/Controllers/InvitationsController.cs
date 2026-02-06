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
public class InvitationsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public InvitationsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim!);
    }

    private string GetCurrentUserEmail()
    {
        return User.FindFirstValue(ClaimTypes.Email)!;
    }

    /// <summary>
    /// Gets all pending invitations for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MyInvitationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyInvitations()
    {
        var email = GetCurrentUserEmail();
        var result = await _teamService.GetMyInvitationsAsync(email);
        return result.ToActionResult();
    }

    /// <summary>
    /// Accepts a team invitation using the token
    /// </summary>
    [HttpPost("accept")]
    [ProducesResponseType(typeof(TeamMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptInvitation(AcceptInvitationDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _teamService.AcceptInvitationAsync(dto.Token, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Declines a team invitation
    /// </summary>
    [HttpPost("{invitationId}/decline")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeclineInvitation(int invitationId)
    {
        var userId = GetCurrentUserId();
        var result = await _teamService.DeclineInvitationAsync(invitationId, userId);

        if (result.IsSuccess)
            return NoContent();

        return result.ToActionResult();
    }
}

