# CloudBoard MVP Roadmap - Azure DevOps Boards Clone

**Last Updated:** November 13, 2025
**Purpose:** Personal use & portfolio to showcase software competency
**Target:** Azure DevOps "Boards" tab functionality

---

## Current State Summary

### ✅ What You Have:
- Clean ASP.NET Core 8 + Vue 3 + PostgreSQL stack
- Hierarchical work items (Epic → Feature → PBI → Task + Bugs)
- Functional Kanban board with drag-and-drop
- CRUD operations for Projects, Boards, Work Items
- Docker containerization and CI/CD pipeline
- Solid architecture with proper separation of concerns

### ❌ What's Missing for MVP:
- User authentication & authorization
- Sprint/iteration management
- Backlog view & prioritization
- Work item assignment to users
- Sprint planning capabilities
- Basic reporting (burndown charts)
- Comments/activity tracking

---

## MVP Scope Definition

For a portfolio-worthy MVP that demonstrates Azure DevOps Boards competency:

1. ✅ User can **login** (simple JWT auth)
2. ✅ User can **view their projects**
3. ✅ User can **navigate to boards**
4. ✅ User can **add/edit/delete tasks** (work items)
5. ✅ User can **plan sprints** (create, add items, set dates)
6. ✅ User can **execute sprints** (track progress, move items)
7. ✅ User can **manage backlog** (prioritize, groom)

---

## Phase 1: Authentication & User Management
**Effort: 8-12 hours | Priority: CRITICAL**

### Backend Tasks:

1. **Add User Entity & Authentication**
   - Create `User` model (Id, Email, PasswordHash, Name, CreatedAt)
   - Add `AspNetCore.Identity` NuGet package (free, built-in)
   - Configure JWT authentication in `Program.cs`
   - Create `AuthController` (Register, Login endpoints)
   - Add `[Authorize]` attributes to existing controllers

2. **Add User Relationships**
   - `Project.OwnerId` (FK to User)
   - `WorkItem.AssignedToId` (FK to User, nullable)
   - `WorkItem.CreatedById` (FK to User)
   - Update DTOs to include user information

3. **Database Migration**
   ```bash
   dotnet ef migrations add AddUserAuthentication
   dotnet ef database update
   ```

### Frontend Tasks:

1. **Login/Register Pages**
   - Create `views/LoginView.vue` with email/password form
   - Create `views/RegisterView.vue`
   - Add auth routes in router
   - Implement JWT token storage (localStorage)

2. **Auth Interceptor**
   - Update `services/api.ts` to inject JWT in headers
   - Add 401 redirect to login page
   - Add token refresh logic (optional for MVP)

3. **User Context**
   - Create Pinia store `stores/auth.ts` for current user
   - Add user dropdown in Sidebar with Logout option
   - Update UI to show assigned user on work items

### Cost Impact:
**$0** - Uses built-in ASP.NET Core Identity, no third-party services

---

## Phase 2: Sprint/Iteration System
**Effort: 16-20 hours | Priority: CRITICAL**

### Backend Tasks:

1. **Add Sprint Entity**
   ```csharp
   public class Sprint {
     int Id
     string Name
     DateTime StartDate
     DateTime EndDate
     string Goal (nullable)
     SprintStatus Status (enum: Planning, Active, Completed)
     int BoardId (FK)
     Board Board
     ICollection<WorkItem> WorkItems
   }
   ```

2. **Update WorkItem Entity**
   - Add `SprintId` (nullable FK) to WorkItem
   - Add navigation property `Sprint`
   - Work items without SprintId are in "Backlog"

3. **Create SprintsController**
   - `GET /api/boards/{boardId}/sprints` - List all sprints
   - `POST /api/boards/{boardId}/sprints` - Create sprint
   - `PUT /api/sprints/{id}` - Update sprint (name, dates, goal)
   - `PATCH /api/sprints/{id}/start` - Start sprint (change status)
   - `PATCH /api/sprints/{id}/complete` - Complete sprint
   - `DELETE /api/sprints/{id}` - Delete sprint (move items to backlog)

4. **Add Sprint Analytics Endpoints**
   - `GET /api/sprints/{id}/stats` - Return completed/in-progress/todo counts
   - `GET /api/sprints/{id}/burndown` - Return daily burndown data

5. **Update WorkItems Endpoints**
   - `PATCH /api/workitems/{id}/assign-sprint` - Move item to/from sprint
   - Update GET endpoints to filter by sprintId or backlog

### Frontend Tasks:

1. **Sprint Management UI**
   - Create `components/sprint/SprintSelector.vue` (dropdown)
   - Create `components/sprint/SprintModal.vue` (create/edit form)
   - Add "Create Sprint" button in BoardDetailView
   - Add "Start Sprint" / "Complete Sprint" actions

2. **Sprint Planning View**
   - Create `views/SprintPlanningView.vue`
   - Left panel: Backlog items (prioritized list)
   - Right panel: Sprint items (drag target)
   - Drag-and-drop from backlog to sprint
   - Show sprint capacity indicator (total estimated hours)

3. **Update BoardDetailView**
   - Add sprint selector dropdown (show current/active sprint)
   - Add "View Backlog" option in selector
   - Filter displayed work items by selected sprint/backlog
   - Add sprint goal display at top

4. **Sprint Types & Models**
   - Create `types/Sprint.ts` with interfaces
   - Add sprint API methods to `services/api.ts`
   - Create composable `composables/useSprints.ts`

### Cost Impact:
**$0** - All custom code, no external services

---

## Phase 3: Backlog Management View
**Effort: 10-14 hours | Priority: HIGH**

### Backend Tasks:

1. **Backlog Prioritization**
   - Add `WorkItem.BacklogPriority` (int, nullable)
   - Lower number = higher priority
   - Update `WorkItemsController`:
     - `PATCH /api/workitems/{id}/reorder` - Update priority
     - Modify GET backlog endpoint to order by priority

2. **Backlog Filtering**
   - Add query parameters to GET endpoints:
     - `?type=Epic,Feature,PBI` (filter by types)
     - `?status=ToDo,InProgress` (filter by status)
     - `?unassigned=true` (show unassigned only)

### Frontend Tasks:

1. **Backlog View Component**
   - Create `views/BacklogView.vue`
   - Hierarchical tree view (Epic → Feature → PBI → Task)
   - Collapsible/expandable items
   - Drag-and-drop to reorder within same parent
   - Inline editing for title/priority

2. **Backlog Actions**
   - Bulk select items → "Add to Sprint" action
   - Quick actions: Edit, Delete, Move to Sprint
   - Filter panel (by type, status, assignee)
   - Search bar integration

3. **Navigation Update**
   - Add "Backlog" tab/button in project view
   - Route: `/projects/:id/backlog`
   - Link from BoardDetailView

### Cost Impact:
**$0** - All custom development

---

## Phase 4: Enhanced Board Views
**Effort: 8-10 hours | Priority: MEDIUM**

### Frontend Tasks:

1. **Sprint Board Enhancements**
   - Add "Sprint Goal" display at top
   - Add sprint date range display (e.g., "Feb 1 - Feb 14")
   - Add progress indicator (% complete based on done items)
   - Add "Days Remaining" countdown

2. **Work Item Card Improvements**
   - Add assignee avatar/initials on cards
   - Add story points display (if estimated hours exist)
   - Add "blocked" indicator (optional)
   - Add quick assign dropdown on card hover

3. **Swimlanes (Optional)**
   - Group by: Assignee, Priority, or Work Item Type
   - Toggle swimlane mode vs. flat view

### Backend Tasks:
- **No backend changes needed** - uses existing data

### Cost Impact:
**$0** - Pure frontend work

---

## Phase 5: Basic Reporting
**Effort: 12-16 hours | Priority: MEDIUM**

### Backend Tasks:

1. **Sprint Burndown API**
   ```csharp
   GET /api/sprints/{id}/burndown
   Returns: [
     { date: "2025-02-01", remainingHours: 120 },
     { date: "2025-02-02", remainingHours: 110 },
     ...
   ]
   ```
   - Calculate total estimated hours at sprint start
   - Track daily completion (sum of done items' estimated hours)
   - Return time series data

2. **Sprint Velocity API**
   ```csharp
   GET /api/boards/{boardId}/velocity?lastN=5
   Returns: [
     { sprintName: "Sprint 1", completedHours: 45 },
     { sprintName: "Sprint 2", completedHours: 52 },
     ...
   ]
   ```

3. **Dashboard Stats API**
   ```csharp
   GET /api/boards/{boardId}/stats
   Returns: {
     totalItems: 45,
     todoCount: 15,
     inProgressCount: 8,
     doneCount: 22,
     overdueCount: 3
   }
   ```

### Frontend Tasks:

1. **Burndown Chart Component**
   - Install `chart.js` and `vue-chartjs` (free, ~100KB)
   - Create `components/charts/BurndownChart.vue`
   - Line chart: Ideal vs. Actual remaining work
   - Display in SprintPlanningView or modal

2. **Velocity Chart Component**
   - Create `components/charts/VelocityChart.vue`
   - Bar chart: Completed hours per sprint
   - Show average velocity line

3. **Dashboard Widgets**
   - Add stats cards to SummaryView
   - Add mini burndown chart to active sprint view
   - Add "Overdue Items" warning widget

### Cost Impact:
**$0** - Chart.js is free and open-source

---

## Phase 6: Comments & Activity (Optional Polish)
**Effort: 10-12 hours | Priority: LOW (Post-MVP)**

### Backend Tasks:

1. **Add Comment Entity**
   ```csharp
   public class Comment {
     int Id
     string Content
     DateTime CreatedAt
     int WorkItemId (FK)
     int UserId (FK)
     User User
     WorkItem WorkItem
   }
   ```

2. **Comments Controller**
   - `GET /api/workitems/{id}/comments`
   - `POST /api/workitems/{id}/comments`
   - `DELETE /api/comments/{id}`

3. **Activity Log** (Simple approach)
   - Add `ActivityLog` table (Id, Message, CreatedAt, UserId, WorkItemId)
   - Log key actions: created, status changed, assigned, moved sprint
   - `GET /api/workitems/{id}/activity`

### Frontend Tasks:

1. **Comment Section**
   - Add collapsible "Comments" section in WorkItemModal
   - List comments with user name and timestamp
   - Add comment textarea with "Post" button

2. **Activity Feed**
   - Add "Activity" tab in WorkItemModal
   - Show chronological list of changes

### Cost Impact:
**$0** - All custom code

---

## Technical Recommendations

### Database (Keep PostgreSQL ✅)
- **Current**: PostgreSQL 16 via Docker
- **Cost**: $0 (local) or ~$5/month (managed hosting like Neon.tech free tier)
- **Why**: Already set up, great for relational data

### Authentication (Use ASP.NET Core Identity ✅)
- **Recommendation**: Built-in `Microsoft.AspNetCore.Identity` + JWT
- **Cost**: $0
- **Why**: No external services needed, perfect for portfolio
- **Alternatives to avoid**: Auth0, Okta (overkill + cost)

### Real-time Updates (Skip for MVP ⚠️)
- **SignalR**: Free but adds complexity
- **Polling**: Simple, works for single-user
- **Decision**: Manual refresh button for MVP, add later if needed

### Hosting Strategy
- **Backend**: Render.com free tier (512MB RAM, spins down after inactivity)
- **Frontend**: Vercel or Netlify free tier
- **Database**: Neon.tech free tier (3GB, 1 branch)
- **Total Cost**: $0/month for MVP

### File Storage (Skip for MVP)
- Attachments can come in Phase 7 if needed
- Use Cloudinary free tier (25 credits/month) or Azure Blob ($0.02/GB)

---

## Recommended Implementation Order

### Week 1: Authentication Foundation
- **Days 1-2**: Backend auth (User model, JWT, Identity setup)
- **Days 3-4**: Frontend login/register, auth store, protected routes
- **Day 5**: Testing & bug fixes

### Week 2: Sprint System Core
- **Days 1-2**: Sprint entity, database migrations, API endpoints
- **Days 3-4**: Sprint management UI (create, edit, selector)
- **Day 5**: Assign work items to sprint functionality

### Week 3: Sprint Planning & Backlog
- **Days 1-2**: Backlog prioritization backend + API
- **Days 3-4**: Sprint planning view (drag-and-drop backlog → sprint)
- **Day 5**: Backlog view with filtering

### Week 4: Board Enhancements & Reporting
- **Days 1-2**: Sprint board improvements (goal, dates, progress)
- **Days 3-4**: Burndown chart backend + frontend
- **Day 5**: Dashboard stats and velocity chart

### Week 5: Polish & Testing
- **Days 1-2**: Bug fixes, edge cases, validation
- **Days 3-4**: Responsive design refinements
- **Day 5**: Documentation, README, deployment

---

## MVP Feature Checklist

### Core Requirements ✅
- [ ] User login (Phase 1)
- [x] See projects (Already exists)
- [x] Go to boards (Already exists)
- [x] Add/edit/delete tasks (Already exists, enhance in Phase 1 with assignment)
- [ ] Plan sprints (Phase 2)
- [ ] Execute sprints (Phase 2 + 4)
- [ ] Handle backlog (Phase 3)

### Bonus Features (If Time Permits)
- [ ] Burndown charts (Phase 5)
- [ ] Velocity tracking (Phase 5)
- [ ] Comments (Phase 6)
- [ ] Activity log (Phase 6)

---

## Risk Mitigation

### Scope Creep Prevention
- ❌ **Skip**: Custom fields, tags, advanced permissions, webhooks, integrations
- ❌ **Skip**: Email notifications, real-time updates, team management
- ❌ **Skip**: Attachments, advanced reporting (CFD, lead time)
- ✅ **Focus**: Core workflow (login → backlog → sprint planning → execution)

### Known Gotchas
1. **JWT Refresh Tokens**: Skip for MVP, just use long-lived tokens (7 days)
2. **Password Hashing**: Use `PasswordHasher<User>` from Identity, never roll your own
3. **CORS**: Already configured in `Program.cs`, but verify for production domain
4. **EF Core Performance**: Add `.AsNoTracking()` for read-only queries
5. **Drag-and-Drop**: Use existing library in Vue (like `vue-draggable-next`)

---

## Success Criteria for MVP

### Functional
1. User can register, login, and stay authenticated
2. User can create a project with multiple sprints
3. User can add work items to backlog
4. User can drag items from backlog into a sprint
5. User can move items across statuses (To Do → In Progress → Done)
6. User can see sprint progress and remaining work
7. User can complete a sprint and start a new one

### Portfolio Value
1. Clean, responsive UI (Tailwind already provides this ✅)
2. Proper REST API design with DTOs
3. Database normalization with relationships
4. Authentication & authorization
5. Complex frontend state management (Pinia)
6. Hierarchical data structures
7. Docker containerization
8. CI/CD pipeline (already exists ✅)

### Non-Functional
1. Loads in < 2 seconds
2. No console errors
3. Works on mobile (responsive)
4. Basic error handling (user-friendly messages)

---

## Estimated Total Effort

| Phase | Hours | Calendar Days |
|-------|-------|---------------|
| Phase 1: Auth | 10h | 2-3 days |
| Phase 2: Sprints | 18h | 3-4 days |
| Phase 3: Backlog | 12h | 2-3 days |
| Phase 4: Board Polish | 9h | 1-2 days |
| Phase 5: Reporting | 14h | 2-3 days |
| **Total Core MVP** | **63h** | **~3 weeks** |
| Phase 6: Comments (Optional) | 11h | 2 days |
| **Total with Polish** | **74h** | **~4 weeks** |

*Assumes 3-4 hours of focused work per day*

---

## Final Recommendations

### Prioritize These for Portfolio Impact:
1. ✅ **Authentication** - Shows you understand security fundamentals
2. ✅ **Sprint Planning View** - Visually impressive with drag-and-drop
3. ✅ **Burndown Chart** - Demonstrates data visualization skills
4. ✅ **Hierarchical Backlog** - Shows you can handle complex data structures

### Skip These to Save Time:
1. ❌ Real-time collaboration (WebSockets/SignalR)
2. ❌ Advanced permissions (organizations, teams, roles)
3. ❌ Email notifications
4. ❌ Custom fields
5. ❌ Attachments

### Document These Well:
- Architecture diagram (frontend ↔️ API ↔️ database)
- ER diagram with relationships
- API documentation (Swagger already configured ✅)
- Demo video or screenshots for README

---

## Next Steps

1. **Review this plan** and decide which phases to include in your MVP
2. **Set up a project board** (ironically, use your own app once auth is done!)
3. **Start with Phase 1** - authentication is foundational for everything else
4. **Commit after each feature** - good Git history is portfolio gold
5. **Deploy early** - test on Render's free tier to catch environment issues

---

## Key File References

### Backend Core
- `api/CloudBoard.Api/Program.cs` - Startup configuration
- `api/CloudBoard.Api/Data/CloudBoardContext.cs` - EF Core context
- `api/CloudBoard.Api/Models/WorkItem.cs` - Core entity with hierarchy

### Frontend Core
- `frontend/src/App.vue` - Root component
- `frontend/src/components/BoardDetailView.vue` - Kanban board
- `frontend/src/services/api.ts` - API client
- `frontend/src/types/WorkItem.ts` - Type definitions

### Configuration
- `docker-compose.yml` - Local development setup
- `.github/workflows/build.yml` - CI/CD pipeline
