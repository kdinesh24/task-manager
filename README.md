# Workflow Manager

A modern full-stack task management application built to master **.NET 8** development through hands-on practice.

> **Author:** Dinesh Kondapalli  
> **Purpose:** Learning & Portfolio Project  
> **Repository:** [github.com/dinesh-kondapalli/task-manager](https://github.com/dinesh-kondapalli/task-manager)

---

## Table of Contents

- [A. High-Level Architecture](#a-high-level-architecture)
- [B. Folder & File Map](#b-folder--file-map)
- [C. Data & Database Documentation](#c-data--database-documentation)
- [D. Business Logic Explanation](#d-business-logic-explanation)
- [E. API Reference](#e-api-reference)
- [F. Setup & Deployment Notes](#f-setup--deployment-notes)
- [G. Testing](#g-testing)

---

## A. High-Level Architecture

### Overall System Structure

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              WORKFLOW MANAGER                                    │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│                              ┌───────────────┐                                  │
│                              │     USER      │                                  │
│                              │   (Browser)   │                                  │
│                              └───────┬───────┘                                  │
│                                      │                                          │
│                                      ▼                                          │
│   ┌──────────────────────────────────────────────────────────────────────────┐ │
│   │                         PRESENTATION LAYER                                │ │
│   │  ┌────────────────────────────────────────────────────────────────────┐  │ │
│   │  │                     Vue.js 3 Frontend                              │  │ │
│   │  │                                                                    │  │ │
│   │  │   • WorkflowBoard.vue  - Main task board UI                       │  │ │
│   │  │   • TaskCard.vue       - Individual task display                  │  │ │
│   │  │   • AddTaskDialog.vue  - Task creation modal                      │  │ │
│   │  │   • api.ts             - HTTP client for backend                  │  │ │
│   │  │                                                                    │  │ │
│   │  │   Port: 5173 (dev) | Served by .NET in production                 │  │ │
│   │  └────────────────────────────────────────────────────────────────────┘  │ │
│   └──────────────────────────────────────────────────────────────────────────┘ │
│                                      │                                          │
│                                      │ HTTP/JSON (REST API)                     │
│                                      ▼                                          │
│   ┌──────────────────────────────────────────────────────────────────────────┐ │
│   │                           API LAYER                                       │ │
│   │  ┌────────────────────────────────────────────────────────────────────┐  │ │
│   │  │                   ASP.NET Core 8 Web API                           │  │ │
│   │  │                                                                    │  │ │
│   │  │   • TasksController.cs  - REST endpoints                          │  │ │
│   │  │   • Program.cs          - App configuration & DI                  │  │ │
│   │  │   • DTOs                - Request/Response models                 │  │ │
│   │  │                                                                    │  │ │
│   │  │   Port: 5000                                                       │  │ │
│   │  └────────────────────────────────────────────────────────────────────┘  │ │
│   └──────────────────────────────────────────────────────────────────────────┘ │
│                                      │                                          │
│                                      │ Entity Framework Core                    │
│                                      ▼                                          │
│   ┌──────────────────────────────────────────────────────────────────────────┐ │
│   │                          DATA LAYER                                       │ │
│   │  ┌────────────────────────────────────────────────────────────────────┐  │ │
│   │  │                      PostgreSQL 16                                 │  │ │
│   │  │                                                                    │  │ │
│   │  │   • tasks table     - Core task storage                           │  │ │
│   │  │   • EF Migrations   - Schema version control                      │  │ │
│   │  │                                                                    │  │ │
│   │  │   Port: 5432                                                       │  │ │
│   │  └────────────────────────────────────────────────────────────────────┘  │ │
│   └──────────────────────────────────────────────────────────────────────────┘ │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### Entry Points

| Entry Point | Location | Purpose |
|-------------|----------|---------|
| **Frontend** | `client/src/main.ts` | Vue.js application bootstrap |
| **Backend** | `server/Program.cs` | .NET application entry, DI configuration, middleware setup |
| **Database** | `docker-compose.yml` | PostgreSQL container initialization |

### Key Subsystems

| Subsystem | Technology | Responsibility |
|-----------|------------|----------------|
| **UI Layer** | Vue.js 3 + TypeScript | User interface, state management, API calls |
| **API Layer** | ASP.NET Core 8 | REST endpoints, request validation, business logic |
| **ORM Layer** | Entity Framework Core 8 | Database abstraction, migrations, LINQ queries |
| **Database** | PostgreSQL 16 | Data persistence, constraints, defaults |
| **Testing** | xUnit + EF InMemory | Unit tests, integration tests |
| **Containerization** | Docker + Compose | Deployment, environment consistency |

---

## B. Folder & File Map

### Root Directory Structure

```
workflow/
│
├── server/                 # [CORE] .NET 8 Backend API
├── server.Tests/           # [CORE] Test project
├── client/                 # [CORE] Vue.js 3 Frontend
│
├── Workflow.sln            # [CONFIG] Visual Studio solution file
├── docker-compose.yml      # [CONFIG] Multi-container Docker setup
├── Dockerfile              # [CONFIG] Production container build
├── .gitignore              # [CONFIG] Git ignore rules
└── README.md               # [DOCS] This file
```

### Server Directory (Backend)

```
server/
│
├── Controllers/                    # [CORE] API endpoint handlers
│   └── TasksController.cs          #   └── All task CRUD operations
│
├── Models/                         # [CORE] Domain entities
│   └── WorkflowTask.cs             #   └── Task entity definition
│
├── Data/                           # [CORE] Database layer
│   └── WorkflowDbContext.cs        #   └── EF Core context & configuration
│
├── Migrations/                     # [AUTO-GENERATED] EF Core migrations
│   ├── 20251128074445_InitialCreate.cs
│   ├── 20251128074445_InitialCreate.Designer.cs
│   └── WorkflowDbContextModelSnapshot.cs
│
├── Properties/                     # [CONFIG] Launch settings
│   └── launchSettings.json
│
├── Program.cs                      # [CORE] Application entry point
├── appsettings.json                # [CONFIG] Production settings
├── appsettings.Development.json    # [CONFIG] Development settings
└── server.csproj                   # [CONFIG] Project dependencies
```

### Server.Tests Directory (Testing)

```
server.Tests/
│
├── Controllers/                    # [CORE] Controller unit tests
│   └── TasksControllerTests.cs     #   └── 35 tests for API endpoints
│
├── Models/                         # [CORE] Model validation tests
│   └── WorkflowTaskTests.cs        #   └── 19 tests for entity behavior
│
├── Data/                           # [CORE] DbContext tests
│   └── WorkflowDbContextTests.cs   #   └── Database operation tests
│
├── Integration/                    # [CORE] Full pipeline tests
│   └── ProgramTests.cs             #   └── 12 end-to-end tests
│
└── server.Tests.csproj             # [CONFIG] Test project dependencies
```

### Client Directory (Frontend)

```
client/
│
├── src/
│   ├── components/                 # [CORE] Vue components
│   │   ├── WorkflowBoard.vue       #   ├── Main board with 3 columns
│   │   ├── TaskCard.vue            #   ├── Individual task card
│   │   ├── AddTaskDialog.vue       #   ├── Create task modal
│   │   └── ui/                     #   └── [LIBRARY] Reusable UI components
│   │
│   ├── services/                   # [CORE] External integrations
│   │   └── api.ts                  #   └── Backend API client
│   │
│   ├── lib/                        # [UTILITY] Helper functions
│   │   └── utils.ts
│   │
│   ├── App.vue                     # [CORE] Root component
│   └── main.ts                     # [CORE] Application entry
│
├── package.json                    # [CONFIG] NPM dependencies
├── vite.config.ts                  # [CONFIG] Vite build configuration
├── tsconfig.json                   # [CONFIG] TypeScript settings
└── index.html                      # [CONFIG] HTML template
```

### Folder Classification Legend

| Tag | Meaning |
|-----|---------|
| `[CORE]` | Critical application code - modify with care |
| `[CONFIG]` | Configuration files - environment specific |
| `[AUTO-GENERATED]` | Generated by tools - do not edit manually |
| `[LIBRARY]` | Third-party or reusable components |
| `[UTILITY]` | Helper functions and utilities |
| `[DOCS]` | Documentation files |

### Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| C# Files | PascalCase | `TasksController.cs` |
| C# Classes | PascalCase | `WorkflowTask` |
| C# Methods | PascalCase | `GetTasks()` |
| Vue Components | PascalCase | `TaskCard.vue` |
| TypeScript Files | camelCase | `api.ts` |
| Database Tables | lowercase | `tasks` |
| API Routes | kebab-case | `/api/tasks` |

---

## C. Data & Database Documentation

### Database Schema Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              PostgreSQL Database                             │
│                              Name: workflow                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │                            tasks                                     │  │
│   ├──────────────────┬──────────────────┬────────────────────────────────┤  │
│   │ Column           │ Type             │ Constraints                    │  │
│   ├──────────────────┼──────────────────┼────────────────────────────────┤  │
│   │ Id               │ SERIAL           │ PRIMARY KEY, AUTO INCREMENT    │  │
│   │ Title            │ VARCHAR(200)     │ NOT NULL                       │  │
│   │ Description      │ VARCHAR(1000)    │ NULLABLE                       │  │
│   │ TaskType         │ TEXT             │ NOT NULL, DEFAULT 'immediate'  │  │
│   │ Status           │ TEXT             │ NOT NULL, DEFAULT 'pending'    │  │
│   │ CreatedAt        │ TIMESTAMPTZ      │ NOT NULL, DEFAULT NOW()        │  │
│   │ ScheduledFor     │ TIMESTAMPTZ      │ NULLABLE                       │  │
│   │ IsCompleted      │ BOOLEAN          │ NOT NULL, DEFAULT FALSE        │  │
│   │ CompletedAt      │ TIMESTAMPTZ      │ NULLABLE                       │  │
│   └──────────────────┴──────────────────┴────────────────────────────────┘  │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │                      __EFMigrationsHistory                           │  │
│   │                      (System table - do not modify)                  │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Key Tables

#### `tasks` - Primary Data Table

| Column | Type | Description |
|--------|------|-------------|
| `Id` | SERIAL | Auto-incrementing primary key |
| `Title` | VARCHAR(200) | Task title (required, max 200 chars) |
| `Description` | VARCHAR(1000) | Optional detailed description |
| `TaskType` | TEXT | Either `immediate` or `scheduled` |
| `Status` | TEXT | One of: `pending`, `in_progress`, `completed` |
| `CreatedAt` | TIMESTAMPTZ | Timestamp when task was created |
| `ScheduledFor` | TIMESTAMPTZ | When scheduled task should be done |
| `IsCompleted` | BOOLEAN | Quick flag for completion status |
| `CompletedAt` | TIMESTAMPTZ | When task was marked complete |

### Database Logic

**Default Values (Set by Database):**
```
TaskType    → 'immediate'       -- New tasks are immediate by default
Status      → 'pending'         -- New tasks start as pending
CreatedAt   → CURRENT_TIMESTAMP -- Auto-set on insert
IsCompleted → FALSE             -- New tasks are not complete
```

**Relationships:**
- Single table design (no foreign keys)
- No stored procedures or triggers
- All business logic in application layer

### Entity Framework Configuration

```csharp
// WorkflowDbContext.cs
modelBuilder.Entity<WorkflowTask>(entity =>
{
    entity.ToTable("tasks");
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Description).HasMaxLength(1000);
    entity.Property(e => e.TaskType).IsRequired().HasDefaultValue("immediate");
    entity.Property(e => e.Status).IsRequired().HasDefaultValue("pending");
    entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
});
```

---

## D. Business Logic Explanation

### Task Lifecycle

```
┌─────────────┐
│   CREATE    │
│             │
│ TaskType:   │──── ScheduledFor provided? ───► scheduled
│ (auto-set)  │                            └──► immediate
│             │
│ Status:     │
│ 'pending'   │
└──────┬──────┘
       │
       ▼
┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│   PENDING   │─────►│ IN_PROGRESS │─────►│  COMPLETED  │
│             │      │             │      │             │
│             │      │             │      │ IsCompleted │
│             │──────────────────────────►│   = TRUE    │
│             │      (direct complete)    │ CompletedAt │
└─────────────┘                           │   = NOW()   │
                                          └─────────────┘
```

### Core Business Rules

#### 1. Task Type Determination
```
IF ScheduledFor is provided:
    TaskType = "scheduled"
ELSE:
    TaskType = "immediate"
```

#### 2. Task Completion
```
WHEN marking complete:
    SET IsCompleted = TRUE
    SET Status = "completed"
    SET CompletedAt = UTC Now
```

#### 3. Task Ordering

| View | Order By | Direction |
|------|----------|-----------|
| All Tasks | CreatedAt | Descending (newest first) |
| Immediate | CreatedAt | Descending (newest first) |
| Scheduled | ScheduledFor | Ascending (soonest first) |

### Data Transfer Objects

**CreateTaskDto:**
```csharp
{
    Title: string        // Required
    Description: string? // Optional
    ScheduledFor: Date?  // Optional - determines TaskType
}
```

**UpdateTaskDto:**
```csharp
{
    Title: string?       // Optional
    Description: string? // Optional
    Status: string?      // Optional
    ScheduledFor: Date?  // Optional
}
```

---

## E. API Reference

### Endpoints

| Method | Endpoint | Description | Success | Error |
|:------:|----------|-------------|:-------:|:-----:|
| `GET` | `/api/tasks` | Get all tasks | 200 | - |
| `GET` | `/api/tasks/{id}` | Get single task | 200 | 404 |
| `GET` | `/api/tasks/immediate` | Filter immediate tasks | 200 | - |
| `GET` | `/api/tasks/scheduled` | Filter scheduled tasks | 200 | - |
| `POST` | `/api/tasks` | Create task | 201 | 400 |
| `PUT` | `/api/tasks/{id}` | Update task | 200 | 404 |
| `PUT` | `/api/tasks/{id}/complete` | Complete task | 200 | 404 |
| `DELETE` | `/api/tasks/{id}` | Delete task | 204 | 404 |

### Request/Response Examples

**Create Task:**
```http
POST /api/tasks
Content-Type: application/json

{
    "title": "Complete documentation",
    "description": "Write comprehensive README"
}
```

**Response (201):**
```json
{
    "id": 1,
    "title": "Complete documentation",
    "description": "Write comprehensive README",
    "taskType": "immediate",
    "status": "pending",
    "createdAt": "2024-01-15T10:30:00Z",
    "scheduledFor": null,
    "isCompleted": false,
    "completedAt": null
}
```

---

## F. Setup & Deployment Notes

### Server Requirements

| Requirement | Minimum | Recommended |
|-------------|---------|-------------|
| .NET SDK | 8.0 | 8.0 (LTS) |
| Node.js | 18.x | 20.x (LTS) |
| PostgreSQL | 14 | 16 |
| Docker | 20.x | Latest |
| RAM | 2 GB | 4 GB |

### Environment Variables

| Variable | Location | Default |
|----------|----------|---------|
| `ConnectionStrings:DefaultConnection` | appsettings.json | `Host=localhost;Database=workflow;Username=postgres;Password=postgres` |
| `ASPNETCORE_ENVIRONMENT` | Environment | `Development` |
| `POSTGRES_DB` | docker-compose.yml | `workflow` |
| `POSTGRES_USER` | docker-compose.yml | `postgres` |
| `POSTGRES_PASSWORD` | docker-compose.yml | `postgres` |

### Quick Start

**Option 1: Docker (Recommended)**
```bash
docker-compose up --build
# App: http://localhost:8080
```

**Option 2: Local Development**
```bash
# Terminal 1: Database
docker-compose up -d postgres

# Terminal 2: Backend
cd server && dotnet run
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger

# Terminal 3: Frontend
cd client && npm install && npm run dev
# UI: http://localhost:5173
```

### Database Commands

```bash
# Create migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Revert migration
dotnet ef migrations remove
```

---

## G. Testing

### Test Summary

```
Total:     66 tests
Passed:    66 tests
Failed:    0 tests
Duration:  ~18 seconds
```

### Coverage by Category

| Category | Tests | Description |
|----------|:-----:|-------------|
| Controller | 35 | API endpoints, HTTP responses |
| Model | 19 | Validation, property defaults |
| Integration | 12 | Full request pipeline |

### Run Tests

```bash
# All tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific class
dotnet test --filter "FullyQualifiedName~TasksControllerTests"
```

---

## License

Educational project for learning purposes.
