# 6. Repository Pattern Implementation

**Date:** 2026-01-29
**Status:** Accepted

## Context

Services were directly using DbContext for data access. While functional, this:
- Made unit testing require in-memory databases or complex DbContext mocking
- Scattered query logic across services
- Coupled services directly to EF Core

## Decision

Implement the Repository pattern with:
- Generic base `IRepository<T>` / `Repository<T>` for common CRUD operations
- Entity-specific repositories (IProjectRepository, IBoardRepository, IWorkItemRepository, ISprintRepository) for domain queries
- Services depend on repository interfaces, not DbContext

### Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                        Controllers                           │
└──────────────────────────┬───────────────────────────────────┘
                           │
┌──────────────────────────▼───────────────────────────────────┐
│                         Services                             │
│  (Business logic, validation, orchestration)                 │
└──────────────────────────┬───────────────────────────────────┘
                           │
┌──────────────────────────▼───────────────────────────────────┐
│                       Repositories                           │
│  IProjectRepository, IBoardRepository, IWorkItemRepository   │
└──────────────────────────┬───────────────────────────────────┘
                           │
┌──────────────────────────▼───────────────────────────────────┐
│                    CloudBoardContext                         │
│  (EF Core DbContext - handles SaveChanges/transactions)      │
└──────────────────────────────────────────────────────────────┘
```

## Why a Base Class?

The generic base class demonstrates SOLID principles:
- **DRY**: Common CRUD code in one place
- **Open/Closed**: Extend via inheritance, base class unchanged
- **Liskov Substitution**: All repositories interchangeable where IRepository<T> is expected

We considered skipping the base class for simplicity, but the CRUD operations are genuinely repeated and the inheritance is straightforward.

## Why NOT Unit of Work?

DbContext already implements Unit of Work:
- Tracks all changes
- SaveChanges commits atomically
- Wrapping it adds indirection without value

If we needed to coordinate across multiple DbContexts or data stores, UoW would be justified.

## SOLID Principles Demonstrated

| Principle | How We Apply It |
|-----------|-----------------|
| **S**ingle Responsibility | Each repository handles data access for one entity type |
| **O**pen/Closed | Base repository is open for extension (inheritance), closed for modification |
| **L**iskov Substitution | Any IRepository<T> implementation can be swapped without breaking callers |
| **I**nterface Segregation | Small, focused interfaces (IProjectRepository vs monolithic IRepository) |
| **D**ependency Inversion | Services depend on abstractions (interfaces), not concrete repositories |

## Consequences

**Positive:**
- Services easily unit tested with mock repositories
- Query logic centralized and reusable
- Clear separation of data access from business logic
- Consistent data access patterns across the application

**Negative:**
- More files (interfaces + implementations)
- Slight indirection when debugging
- Must remember to call SaveChangesAsync (same as before)

**Trade-off accepted:** The additional files are worth the testability improvement.

## Files Created

| File | Purpose |
|------|---------|
| `Repositories/IRepository.cs` | Generic interface for common CRUD |
| `Repositories/Repository.cs` | Generic implementation |
| `Repositories/IProjectRepository.cs` | Project-specific queries |
| `Repositories/ProjectRepository.cs` | Project implementation |
| `Repositories/IBoardRepository.cs` | Board-specific queries |
| `Repositories/BoardRepository.cs` | Board implementation |
| `Repositories/IWorkItemRepository.cs` | WorkItem-specific queries |
| `Repositories/WorkItemRepository.cs` | WorkItem implementation |
| `Repositories/ISprintRepository.cs` | Sprint-specific queries |
| `Repositories/SprintRepository.cs` | Sprint implementation |
