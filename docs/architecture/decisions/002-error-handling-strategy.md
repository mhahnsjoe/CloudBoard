# 2. Error Handling Strategy

**Date:** 2026-01-28
**Status:** Accepted

## Context

We needed consistent error handling across the API. Options considered:
1. Try-catch in every controller action
2. Global exception middleware with exception-to-HTTP mapping
3. Result<T> pattern returning success/failure from services
4. Combination of middleware + Result pattern

## Decision

We use a **hybrid approach** combining Result<T> pattern in services with global exception middleware.

**Result Pattern:**
- Services return `Result<T>` for operations that can fail in expected ways
- Controllers use `ToActionResult()` extensions to convert Results to HTTP responses
- Provides type-safe error handling without exceptions

**Exception Middleware:**
- Catches unexpected exceptions globally
- Maps specific exceptions to HTTP status codes
- Provides consistent RFC 7807 ProblemDetails responses

**Exception Mappings:**
- `KeyNotFoundException` → 404 Not Found
- `UnauthorizedAccessException` → 403 Forbidden
- `InvalidOperationException` → 400 Bad Request
- Unhandled exceptions → 500 Internal Server Error

## Consequences

**Positive:**
- Controllers stay clean with Result pattern
- Consistent error response format
- Type-safe error handling in services
- Centralized error logging with appropriate severity
- Stack traces hidden in production, shown in development

**Negative:**
- Two error handling approaches (Result for expected, exceptions for unexpected)
- Must remember when to use Result vs when to throw

**Trade-offs:**
The Result pattern is more ceremony than pure exceptions, but provides better type safety and makes error cases explicit. For a portfolio project demonstrating professional practices, this is a worthwhile trade-off.
