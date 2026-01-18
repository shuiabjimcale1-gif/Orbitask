# Orbitask Backend API

A modular, multi‑tenant backend service designed for a modern task‑management platform inspired by tools like Trello and Notion. This backend provides a clean, explicit architecture with strict trust boundaries, predictable CRUD flows, and a Dapper‑first data access layer optimized for performance and maintainability.

## 📌 Project Overview

Orbitask Backend is a RESTful API built with ASP.NET Core, Dapper, and SQL Server, providing a secure and scalable foundation for managing:

- Workbenches
- Boards
- Columns
- Tasks
- Tags
- Task‑Tag relationships

The system emphasizes explicit data control, multi‑tenant safety, and predictable behavior—no hidden cascades, no ORM magic, and no implicit side effects.

## ✨ Key Features

- Multi‑tenant architecture with strict Workbench → Board → Column → Task hierarchy
- Explicit CRUD operations with manual foreign‑key validation
- High‑performance Dapper data access
- Clean service layer enforcing trust boundaries
- Modular repository pattern for maintainability
- JWT‑ready authentication structure (planned)
- Error‑driven deletion logic (no cascading deletes)
- Serialization‑safe models without navigation properties

## 📦 Installation & Setup

1. **Clone the repository**
```bash
git clone https://github.com/shuiabjimcale1-gif/orbitask-backend.git
cd orbitask-backend
# Orbitask
