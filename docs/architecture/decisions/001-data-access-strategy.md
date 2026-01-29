# 1. Data Access Strategy

**Date:** 2026-01-28
**Status:** Accepted

## Context

We needed to decide how services access the database. Common options:
1. Direct DbContext injection (current approach)
2. Generic Repository pattern (IRepository<T>)
3. Specific Repository per aggregate
4. CQRS with separate read/write models

## Decision

We use **direct DbContext injection** into services with extracted query methods for reusable queries.

Rationale:
- EF Core's DbSet<T> already provides repository-like functionality
- DbContext implements Unit of Work (SaveChanges is atomic)
- Direct access preserves EF Core features (change tracking, Include, projections)
- Query methods provide reuse without full abstraction overhead
- Simpler debugging - no abstraction layers to navigate

## Consequences

**Positive:**
- Less code to maintain
- Full access to EF Core capabilities
- Easier debugging with direct database calls
- Lower learning curve for new developers

**Negative:**
- Services have direct EF Core dependency (acceptable for this project size)
- Unit testing requires in-memory database or mocking DbContext
- If we later need to swap databases, changes touch services directly

**Mitigations:**
- Integration tests with Testcontainers validate actual database behavior
- Query methods can be extracted if patterns emerge
- Service layer still provides business logic isolation from controllers
