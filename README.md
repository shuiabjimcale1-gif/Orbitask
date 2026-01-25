# Orbitask - Multi-Tenant Task Management Platform

> A production-grade kanban-style task management system with workbench-level isolation, role-based access control, and hierarchical data organization.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/en-us/sql-server)
[![JWT](https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens)](https://jwt.io/)

## 🎯 Overview

Orbitask is a multi-tenant SaaS application built from scratch to demonstrate enterprise-level architecture patterns. It implements a hierarchical task management system with workbench-level tenant isolation, three-tier role-based permissions, and a normalized relational data model.

**Key Highlights:**
- Multi-tenant architecture with complete data isolation
- Role-based access control (Owner, Admin, Member)
- Hierarchical data model: Workbench → Board → Column → Task
- JWT-based authentication
- Clean three-tier architecture (Controller → Service → Data)
- Normalized database design with JOIN-based authorization

---

## ✨ Features

### Multi-Tenancy
- **Workbench Isolation**: Complete data separation between tenants
- **Membership Management**: Invite users with specific roles
- **Cross-Tenant Security**: Automatic tenancy wall prevents data leaks

### Role-Based Access Control
- **Owner (Role 0)**: Billing authority, can delete workbench, full control
- **Admin (Role 1)**: Manage boards and members, cannot delete workbench
- **Member (Role 2)**: Create/edit tasks, limited permissions
- **Auto-Succession**: Admin promoted to Owner when Owner leaves

### Task Management
- **Boards**: Organize work by project or team
- **Columns**: Kanban-style workflow stages
- **Tasks**: Individual work items with metadata
- **Tags**: Board-scoped labels for categorization
- **Cross-Board Prevention**: Tasks and tags cannot cross board boundaries

### Security Features
- **Tenancy Wall**: Users only see data from their workbenches
- **Hierarchical Authorization**: Permissions derived from workbench membership
- **Database Constraints**: Single Owner per workbench enforced at DB level
- **JWT Authentication**: Secure token-based auth with ASP.NET Identity

---

## 🏗️ Architecture

### System Design

```
┌─────────────────────────────────────────────────────────┐
│                    MULTI-TENANT SYSTEM                  │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  Workbench (Tenant Root)                               │
│  ├── WorkbenchMembers (RBAC: Owner/Admin/Member)       │
│  └── Boards                                            │
│      ├── Columns                                       │
│      │   └── Tasks                                     │
│      │       └── TaskTags (Many-to-Many)              │
│      └── Tags (Board-scoped)                          │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### Three-Tier Architecture

```
┌──────────────┐
│ Controllers  │  ← Authorization (check workbench membership)
└──────┬───────┘
       │
┌──────▼───────┐
│  Services    │  ← Business Logic (validate, orchestrate)
└──────┬───────┘
       │
┌──────▼───────┐
│  Data Layer  │  ← SQL Execution (Dapper for performance)
└──────────────┘
```

### Security Model: "Two Birds One Stone"

```csharp
// One query checks BOTH tenancy AND authorization
var membership = await GetMembership(workbenchId, userId);

if (membership == null)
    return Forbid();  // Tenancy wall: not in this workbench

if (membership.Role != WorkbenchRole.Admin)
    return Forbid();  // RBAC: insufficient permissions
```

---

## 🛠️ Tech Stack

### Backend
- **ASP.NET Core 8.0** - Web API framework
- **C# 12** - Programming language
- **Dapper** - Micro-ORM for high-performance data access
- **Entity Framework Core** - For Identity management
- **ASP.NET Identity** - User authentication and management

### Database
- **SQL Server 2022** - Relational database
- **Normalized Schema** - Third normal form with JOIN-based queries

### Authentication
- **JWT (JSON Web Tokens)** - Stateless authentication
- **BCrypt** - Password hashing

### Tools
- **Visual Studio 2022** / **VS Code** - Development environment
- **Postman** / **Swagger** - API testing
- **Git** - Version control

---

## 📋 Prerequisites

Before you begin, ensure you have the following installed:

### Required Software

1. **.NET 8.0 SDK**
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: `dotnet --version`

2. **SQL Server 2022** (or SQL Server Express)
   - Download: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   - Alternative: Use Docker for SQL Server

3. **Git**
   - Download: https://git-scm.com/downloads
   - Verify installation: `git --version`

### Recommended Software

4. **Visual Studio 2022** (Community Edition is free)
   - Download: https://visualstudio.microsoft.com/downloads/
   - Workload: "ASP.NET and web development"

5. **SQL Server Management Studio (SSMS)**
   - Download: https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms
   - For database management and querying

6. **Postman** (for API testing)
   - Download: https://www.postman.com/downloads/

---

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/orbitask.git
cd orbitask
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

### 3. Configure Database Connection

Edit `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OrbitaskDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**For SQL Server Authentication:**
```json
"DefaultConnection": "Server=localhost;Database=OrbitaskDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
```

### 4. Configure JWT Settings

In `appsettings.json`, set your JWT secret key:

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyHere_AtLeast32Characters!",
    "Issuer": "OrbitaskAPI",
    "Audience": "OrbitaskClients"
  }
}
```

**⚠️ IMPORTANT:** Use a strong, unique key for production!

---

## 🗄️ Database Setup

### Option A: Using Entity Framework Migrations

```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to database
dotnet ef database update
```

### Option B: Using SQL Scripts

1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server instance
3. Create a new database:
   ```sql
   CREATE DATABASE OrbitaskDB;
   ```
4. Run the migration scripts in order (if provided in `/Database` folder)

### Verify Database Creation

```sql
USE OrbitaskDB;

-- Check tables were created
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';

-- Expected tables:
-- Workbenches, WorkbenchMembers, Boards, Columns, 
-- TaskItems, Tags, TaskTags, AspNetUsers, etc.
```

---

## ▶️ Running the Application

### Development Mode

```bash
# Run with hot reload
dotnet watch run

# Or run normally
dotnet run
```

The API will start at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`

### Swagger UI

Navigate to:
```
https://localhost:5001/swagger
```

This provides interactive API documentation and testing.

---

## 📚 API Documentation

### Base URL
```
https://localhost:5001/api
```

### Authentication Endpoints

#### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": { ... }
}
```

### Workbench Endpoints

#### Create Workbench (become Owner)
```http
POST /api/workbenches
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "My Team Workspace"
}
```

#### Get My Workbenches
```http
GET /api/workbenches
Authorization: Bearer {token}
```

#### Get Single Workbench
```http
GET /api/workbenches/{workbenchId}
Authorization: Bearer {token}
```

#### Delete Workbench (Owner only)
```http
DELETE /api/workbenches/{workbenchId}
Authorization: Bearer {token}
```

### Board Endpoints

```http
GET    /api/workbenches/{workbenchId}/boards
POST   /api/workbenches/{workbenchId}/boards
PUT    /api/boards/{boardId}
DELETE /api/boards/{boardId}
```

### Column Endpoints

```http
GET    /api/boards/{boardId}/columns
POST   /api/boards/{boardId}/columns
PUT    /api/columns/{columnId}
DELETE /api/columns/{columnId}
```

### Task Endpoints

```http
GET    /api/columns/{columnId}/tasks
POST   /api/columns/{columnId}/tasks
PUT    /api/tasks/{taskId}
DELETE /api/tasks/{taskId}

POST   /api/tasks/{taskId}/tags/{tagId}    # Attach tag
DELETE /api/tasks/{taskId}/tags/{tagId}    # Remove tag
```

### Member Management

```http
GET    /api/workbenches/{workbenchId}/members
POST   /api/workbenches/{workbenchId}/members
PUT    /api/workbenches/{workbenchId}/members/{userId}
DELETE /api/workbenches/{workbenchId}/members/{userId}
```

For complete API documentation, visit `/swagger` when the application is running.

---

## 🔒 Security Model

### Role Hierarchy

| Role | Delete Workbench | Manage Boards | Invite Users | Edit Tasks |
|------|------------------|---------------|--------------|------------|
| **Owner** | ✅ | ✅ | ✅ | ✅ |
| **Admin** | ❌ | ✅ | ✅ | ✅ |
| **Member** | ❌ | ❌ | ❌ | ✅ |

### Owner Role Rules

- **Automatic Assignment**: Creator becomes Owner on workbench creation
- **Single Owner**: Database enforces exactly ONE Owner per workbench
- **Cannot Be Removed**: Only Owner can leave workbench
- **Auto-Succession**: When Owner leaves, first Admin promoted to Owner
- **Delete Permission**: Only Owner can delete workbench

### Tenancy Isolation

```
User A's Workbench ─┐
                    ├─ User A can access
                    └─ User B CANNOT access (403 Forbidden)

User B's Workbench ─┐
                    ├─ User B can access
                    └─ User A CANNOT access (403 Forbidden)
```

### Authorization Flow

```
1. User makes request → Extract JWT token
2. Validate token → Get userId
3. Check membership → WorkbenchMembers table
   - If NULL → 403 Forbidden (tenancy wall)
   - If exists → Check role for operation
4. Proceed with request if authorized
```

---

## 📁 Project Structure

```
Orbitask/
├── Controllers/
│   ├── AuthController.cs          # Authentication endpoints
│   ├── WorkbenchController.cs     # Workbench + member management
│   ├── BoardController.cs         # Board CRUD
│   ├── ColumnController.cs        # Column CRUD
│   ├── TaskItemController.cs      # Task CRUD + tag attachment
│   └── TagController.cs           # Tag CRUD
│
├── Services/
│   ├── Interfaces/
│   │   ├── IWorkbenchService.cs
│   │   ├── IBoardService.cs
│   │   ├── IColumnService.cs
│   │   ├── ITaskItemService.cs
│   │   └── ITagService.cs
│   │
│   ├── WorkbenchService.cs        # Workbench business logic
│   ├── BoardService.cs
│   ├── ColumnService.cs
│   ├── TaskItemService.cs
│   └── TagService.cs
│
├── Data/
│   ├── Interfaces/
│   │   ├── IWorkbenchData.cs
│   │   ├── IBoardData.cs
│   │   ├── IColumnData.cs
│   │   ├── ITaskItemData.cs
│   │   └── ITagData.cs
│   │
│   ├── WorkbenchData.cs           # SQL queries (Dapper)
│   ├── BoardData.cs
│   ├── ColumnData.cs
│   ├── TaskItemData.cs
│   └── TagData.cs
│
├── Models/
│   ├── User.cs                    # ASP.NET Identity user
│   ├── Workbench.cs               # Tenant root
│   ├── WorkbenchMember.cs         # RBAC membership
│   ├── Board.cs
│   ├── Column.cs
│   ├── TaskItem.cs
│   ├── Tag.cs
│   └── TaskTag.cs                 # Many-to-many join
│
├── Database/
│   └── ApplicationDbContext.cs    # EF Core context
│
├── Migrations/                    # EF migrations
│
├── appsettings.json               # Configuration
├── appsettings.Development.json
├── Program.cs                     # Application entry point
└── README.md                      # This file
```

---

## 🧪 Testing

### Manual Testing with Postman

1. **Import the Postman collection** (if provided in `/docs`)
2. **Set environment variables**:
   - `baseUrl`: `https://localhost:5001/api`
   - `token`: (obtained from login)

### Example Test Flow

```bash
# 1. Register a user
POST /api/auth/register

# 2. Login
POST /api/auth/login
# Save the token from response

# 3. Create workbench (you become Owner)
POST /api/workbenches
Authorization: Bearer {token}

# 4. Create board
POST /api/workbenches/1/boards

# 5. Create column
POST /api/boards/1/columns

# 6. Create task
POST /api/columns/1/tasks

# 7. Invite another user
POST /api/workbenches/1/members
{
  "userId": "other-user-id",
  "role": 1  // Admin
}
```

---

## 🎓 Key Design Decisions

### 1. Normalization Over Denormalization

**Initial Design**: Stored redundant `WorkbenchId` on all entities for fast lookups.

**Refactored**: Removed redundancy, derive `WorkbenchId` via JOINs.

**Why**: 
- Guarantees data integrity (single source of truth)
- No risk of FK mismatch
- Minimal performance impact with proper indexes

### 2. Owner Role Instead of Just Admin

**Why**:
- Prepares for billing (Owner = person who pays)
- Prevents accidental workbench deletion
- Clear authority hierarchy

### 3. Dapper for Data Layer

**Why**:
- Performance (direct SQL execution)
- Fine-grained control over queries
- Learning opportunity (SQL skills)

### 4. No Navigation Properties

**Why**:
- Explicit about data loading
- No lazy loading surprises
- Clear about what each query returns

---

## 🤝 Contributing

This is a learning project, but contributions are welcome!

### How to Contribute

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Style

- Follow C# naming conventions
- Add XML comments to public methods
- Keep controllers thin (delegate to services)
- Write business logic in services
- Keep data layer focused on SQL only

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Built as a learning project to demonstrate enterprise architecture patterns
- Inspired by production SaaS platforms like Trello, Asana, and Notion
- Special thanks to the .NET community for excellent documentation

---

## 📧 Contact

**GitHub**: https://github.com/shuiabjimcale1-gif/orbitask

**Issues**: https://github.com/shuiabjimcale1-gif/orbitask/issues

---

## 🗺️ Roadmap

### Phase 1 (Current)
- ✅ Multi-tenant architecture
- ✅ RBAC with Owner role
- ✅ Complete CRUD operations
- ✅ JWT authentication

### Phase 2 (Planned)
- [ ] Ownership transfer endpoint
- [ ] Stripe billing integration
- [ ] Email notifications
- [ ] Audit logging

### Phase 3 (Future)
- [ ] Real-time updates (SignalR)
- [ ] File attachments
- [ ] Activity feeds
- [ ] API rate limiting

---

## 💡 Learning Resources

If you're studying this project, here are key concepts to understand:

1. **Multi-Tenancy**: [Microsoft Docs - Multi-tenant SaaS](https://docs.microsoft.com/en-us/azure/architecture/guide/multitenant/overview)
2. **RBAC**: [Role-Based Access Control](https://en.wikipedia.org/wiki/Role-based_access_control)
3. **Database Normalization**: [Database Normal Forms](https://www.guru99.com/database-normalization.html)
4. **JWT**: [Introduction to JWT](https://jwt.io/introduction)
5. **Clean Architecture**: [Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**Built with ❤️ as a learning project to demonstrate production-grade architecture patterns.**
