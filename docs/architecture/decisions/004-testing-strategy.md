# 4. Testing Strategy

**Date:** 2026-01-28
**Status:** Accepted

## Context

We needed a testing approach that provides confidence without excessive maintenance burden.

## Decision

**Backend:**
- Unit tests for validation logic and pure business rules (xUnit, FluentAssertions)
- Integration tests with real PostgreSQL via Testcontainers
- No mocking of DbContext (use in-memory or real database)

**Frontend:**
- Component tests for complex interactions (Vitest, Vue Test Utils)
- No E2E tests initially (manual testing sufficient for portfolio scope)

**Integration Testing Approach:**
- Testcontainers spins up PostgreSQL in Docker
- Each test class shares a database instance
- Tests use real HTTP client against in-memory test server
- Authentication handled once per test class

## Consequences

**Positive:**
- Integration tests catch real database issues (constraints, migrations)
- Testcontainers ensures CI/CD parity with production
- Less brittle than heavily mocked tests
- Validates actual EF Core behavior

**Negative:**
- Integration tests slower than pure unit tests
- Testcontainers requires Docker in CI
- Database startup adds ~2-3 seconds per test run

**Why not mock DbContext:**
Mocking DbContext leads to tests that pass but don't reflect real database behavior (navigation properties, lazy loading, constraints). Integration tests with Testcontainers provide actual confidence.

**Test Coverage Goals:**
- Unit tests: Validation logic, business rules
- Integration tests: End-to-end API flows
- Manual testing: UI flows, edge cases
