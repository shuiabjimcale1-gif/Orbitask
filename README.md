# Orbitask
Orbitask Backend API
A modular, multi‑tenant backend service designed for a modern task‑management platform inspired by tools like Trello and Notion. This backend provides a clean, explicit architecture with strict trust boundaries, predictable CRUD flows, and a Dapper‑first data access layer optimized for performance and maintainability.

📌 Project Overview
Orbitask Backend is a RESTful API built with ASP.NET Core, Dapper, and SQL Server, providing a secure and scalable foundation for managing:

Workbenches

Boards

Columns

Tasks

Tags

Task‑Tag relationships

The system emphasizes explicit data control, multi‑tenant safety, and predictable behavior—no hidden cascades, no ORM magic, and no implicit side effects.

✨ Key Features
Multi‑tenant architecture with strict Workbench → Board → Column → Task hierarchy

Explicit CRUD operations with manual foreign‑key validation

High‑performance Dapper data access

Clean service layer enforcing trust boundaries

Modular repository pattern for maintainability

JWT‑ready authentication structure (planned)

Error‑driven deletion logic (no cascading deletes)

Serialization‑safe models without navigation properties

📦 Installation & Setup
1. Clone the repository
bash
git clone https://github.com/yourusername/orbitask-backend.git
cd orbitask-backend
2. Configure the database
Create a SQL Server database (e.g., OrbitaskDb) and update your appsettings.json:

json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=OrbitaskDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
3. Apply migrations (if using EF for Identity)
bash
dotnet ef database update
4. Restore dependencies
bash
dotnet restore
5. Run the application
bash
dotnet run
API will be available at:

Code
https://localhost:5001
http://localhost:5000
🚀 Usage Examples
Create a Board
http
POST /api/workbenches/{workbenchId}/boards
Content-Type: application/json

{
  "name": "Development Roadmap"
}
Create a Task
http
POST /api/columns/{columnId}/tasks
Content-Type: application/json

{
  "title": "Implement Update Logic",
  "description": "Ensure multi-tenant safety",
  "position": 1
}
Update a Column
http
PUT /api/columns/{columnId}
Content-Type: application/json

{
  "name": "In Progress",
  "position": 2
}
🏗 Architecture Overview
Orbitask follows a clean, layered architecture:

Code
Controllers → Services → Data Layer → SQL Server
Design Principles
Explicit > Implicit

Fail fast on invalid relationships

No navigation properties

No cascading deletes

Manual ID enforcement

Predictable, testable flows

📚 Dependencies & Requirements
Runtime
.NET 8+

SQL Server 2019+

Windows, macOS, or Linux

NuGet Packages
Dapper

Microsoft.Data.SqlClient

Microsoft.AspNetCore.Mvc

Microsoft.Extensions.Configuration

(Optional) Identity EF Core

📬 Contact & Support
Author: Shuiab Jimcale
GitHub: https://github.com/shuiabjimcale1-gif 
Email: shuiabjimcale@gmail.com
