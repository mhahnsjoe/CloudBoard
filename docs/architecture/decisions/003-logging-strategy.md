# 3. Logging Strategy

**Date:** 2026-01-28
**Status:** Accepted

## Context

We needed structured logging for debugging and monitoring. Considerations:
- Log aggregation in production
- Correlation across requests
- Sensitive data handling
- Performance overhead

## Decision

We use **Serilog** with:
- Structured logging (JSON-friendly properties)
- Request logging middleware
- File + Console sinks
- Environment-based minimum levels

We explicitly **do not** implement correlation IDs because:
- CloudBoard is a monolith (single process per request)
- No distributed tracing requirements
- TraceIdentifier from ASP.NET Core suffices for request tracking

**Logging Levels:**
- `LogDebug` - Method entry (not logged in production)
- `LogInformation` - Successful operations
- `LogWarning` - Expected failures (not found, validation errors)
- `LogError` - Unexpected failures (with exception)

## Consequences

**Positive:**
- Queryable logs with structured properties
- Appropriate verbosity per environment
- Request timing out of the box
- Performance optimized for production

**Negative:**
- File logs require rotation management
- No distributed tracing (acceptable for monolith)

**Future consideration:**
If CloudBoard expands to microservices, add OpenTelemetry for distributed tracing rather than custom correlation middleware.
