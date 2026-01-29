# 5. Technical Debt Register

**Date:** 2026-01-28
**Status:** Accepted

## Context

As the codebase evolves, we accumulate technical debt - code that works but could be improved. Rather than leaving TODOs scattered in code, we centralize them here with priority and rationale.

## Documented Technical Debt

### HIGH PRIORITY

None currently.

### MEDIUM PRIORITY

**TD-001: Improve Backlog Item Creation Pattern**
- **Location:** WorkItemService.cs:35
- **Issue:** Using `dto.BoardId == null` to distinguish backlog items is implicit
- **Better approach:** Add `CreateBacklogItemAsync` method or explicit flag
- **Effort:** 2 hours
- **Why deferred:** Current approach works and is well-tested. Not causing issues.

**TD-002: ProjectId Assignment in WorkItem Creation**
- **Location:** WorkItemService.cs:45
- **Issue:** Setting `dto.ProjectId = board.ProjectId` mutates the DTO
- **Better approach:** Query board with ProjectId eagerly or pass separately
- **Effort:** 1 hour
- **Why deferred:** Works correctly, minor code smell. Low priority.

**TD-003: Add Logging to SprintService**
- **Location:** SprintService.cs:13
- **Issue:** SprintService lacks structured logging
- **Better approach:** Add ILogger and log key operations
- **Effort:** 30 minutes
- **Why deferred:** Sprints are a secondary feature. Add when sprint usage increases.

**TD-004: Project Ownership Verification**
- **Location:** WorkItemService.cs:291 (MoveToBoardAsync)
- **Issue:** Missing project ownership check when moving items
- **Better approach:** Verify user owns the target board's project
- **Effort:** 1 hour
- **Why deferred:** Current auth model trusts authenticated users. Add if multi-tenancy becomes stricter.

### LOW PRIORITY

**TD-005: Null-forgiving Operator in Navigation**
- **Location:** WorkItemService.cs:252
- **Issue:** `.ThenInclude(b => b!.Project)` uses null-forgiving operator
- **Better approach:** Review if Board.Project can actually be null, adjust model
- **Effort:** 30 minutes
- **Why deferred:** EF Core navigation quirk. Works correctly at runtime.

**TD-006: AuthService User Retrieval Location**
- **Location:** AuthService.cs:79
- **Issue:** Comment suggests method might belong elsewhere
- **Better approach:** Review if GetCurrentUser fits better in a UserService
- **Effort:** 1 hour
- **Why deferred:** Service separation is adequate for current scope.

## Decision

We maintain this centralized register rather than inline TODOs because:
1. Easier to prioritize and review collectively
2. Can reference in sprint planning
3. Doesn't clutter code
4. Provides context on why debt exists

## Consequences

**Positive:**
- Technical debt is visible and prioritized
- New developers understand trade-offs
- Can plan dedicated cleanup sprints

**Negative:**
- Requires discipline to update this document
- Could become stale if not reviewed periodically

**Review cadence:** Quarterly or before major releases.
