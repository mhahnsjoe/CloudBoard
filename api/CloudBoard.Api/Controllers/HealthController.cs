using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudBoard.Api.Data;
using Asp.Versioning;

namespace CloudBoard.Api.Controllers;

/// <summary>
/// Health check endpoints for monitoring and load balancer probes.
/// Not versioned - health checks should be stable across API versions.
/// </summary>
[ApiController]
[Route("[controller]")]
[ApiVersionNeutral]
public class HealthController : ControllerBase
{
    private readonly CloudBoardContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(CloudBoardContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Basic liveness check - returns 200 if API is running
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = typeof(HealthController).Assembly.GetName().Version?.ToString() ?? "1.0.0"
        });
    }

    /// <summary>
    /// Readiness check - verifies database connectivity
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                _logger.LogWarning("Database connectivity check failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    reason = "Database unavailable",
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                status = "ready",
                database = "connected",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new
            {
                status = "unhealthy",
                reason = "Health check failed",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
