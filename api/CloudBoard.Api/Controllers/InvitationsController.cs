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
}
