# AGENT.md

## Project Overview

Project Name: **TechJournal-Medium**

TechJournal-Medium adalah backend REST API berbasis ASP.NET Core 8 yang mengimplementasikan platform publishing artikel seperti Medium.

Project ini digunakan sebagai sarana pembelajaran dan implementasi praktik backend engineering modern meliputi:

- ASP.NET Core 8
- Entity Framework Core
- SQL Server
- JWT Authentication
- Role Based Access Control (RBAC)
- Controller → Service → Repository Architecture
- Background Jobs menggunakan Coravel
- Validation
- Error Handling
- Query Engineering
- Soft Delete
- Audit Trail

---

# Mission

Bangun REST API yang:

- Clean
- Maintainable
- Testable
- Scalable
- Mengikuti standard engineering .NET DOT

Setiap perubahan kode harus menjaga separation of concerns dan tidak mengorbankan kualitas arsitektur.

---

# Tech Stack

## Runtime

- .NET 8 LTS
- ASP.NET Core Web API

## Database

- SQL Server 2022
- Entity Framework Core 8

## Authentication

- JWT Bearer Authentication
- BCrypt Password Hashing

## Background Processing

- Coravel

## API Documentation

- Swagger / OpenAPI

---

# Architecture Rules

## Layer Flow

Controller

↓

Service

↓

Repository

↓

DbContext

Controller tidak boleh mengakses database secara langsung.

Service tidak boleh mengakses HttpContext.

Repository hanya bertanggung jawab terhadap persistence.

---

# Folder Structure

```text
Medium.Api
│
├── Domain
│   ├── Auth
│   ├── User
│   ├── Article
│   ├── Tag
│   ├── Comment
│   ├── Bookmark
│   └── Follow
│
├── Http
│   └── Api
│       └── V1
│           ├── Auth
│           ├── Users
│           ├── Articles
│           ├── Tags
│           ├── Comments
│           └── Bookmarks
│
├── Infrastructure
│   ├── Database
│   ├── Repositories
│   ├── Middlewares
│   ├── Exceptions
│   ├── Jobs
│   ├── Logging
│   └── Shared
│
├── Models
├── Migrations
├── Constants
├── Properties
└── Program.cs
```

---

# Naming Convention

## File

PascalCase

Examples:

```text
ArticleController.cs
ArticleService.cs
ArticleRepository.cs
JwtTokenGenerator.cs
```

## Classes

PascalCase

```csharp
public class ArticleService
{
}
```

## Interfaces

Prefix with I

```csharp
public interface IArticleService
{
}
```

## Methods

PascalCase

```csharp
public async Task<ArticleDto> GetArticleByIdAsync(Guid articleId)
{
}
```

## Variables

camelCase

```csharp
var publishedArticles = new List<Article>();
```

## Database Tables

snake_case

```sql
users
articles
article_tags
reading_histories
```

## Database Columns

snake_case

```sql
created_at
updated_at
deleted_at
published_at
```

---

# Entity Standards

All entities must inherit from BaseEntity.

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
```

---

# Required Core Entities

## User

Role:

- Reader
- Author
- Admin

## Article

Status:

- Draft
- Scheduled
- Published
- Archived

## Tag

## ArticleTag

Many-to-many

## Comment

## Bookmark

## Follow

## ReadingHistory

## NewsletterSubscription

---

# EF Core Rules

## Migrations

Always generate migration after schema change.

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

## Relationships

Use Fluent API.

Avoid Data Annotation for complex relationship mapping.

## Soft Delete

Every aggregate root must support:

```csharp
DeletedAt
```

Use Global Query Filter.

```csharp
builder.HasQueryFilter(x => x.DeletedAt == null);
```

## Audit Fields

CreatedAt and UpdatedAt must be managed automatically inside DbContext.

---

# DTO Rules

Never expose Entity directly from Controller.

Always return DTO.

Bad:

```csharp
return article;
```

Good:

```csharp
return ArticleResponseDto;
```

---

# Controller Rules

Controller responsibilities:

- Read Request
- Validate Route Parameter
- Call Service
- Return Response

Controller must NOT:

- Query DbContext
- Execute Business Logic
- Access Repository

---

# Service Rules

Service responsibilities:

- Business Logic
- Ownership Validation
- Authorization Logic
- Transaction Coordination

Service must NOT:

- Return Entity Framework tracking objects
- Access HttpContext directly

---

# Repository Rules

Repository responsibilities:

- Query
- Persistence
- Data Access

Repository must NOT:

- Implement Business Rules
- Read JWT Claims

---

# Authentication Rules

Password hashing:

```csharp
BCrypt.Net.BCrypt
```

JWT Claims minimum:

```text
sub
email
role
```

Protected endpoints must use:

```csharp
[Authorize]
```

Role protected endpoints:

```csharp
[Authorize(Roles = "Admin")]
```

---

# Ownership Rules

Author:

- Can edit own article
- Can delete own article

Reader:

- Can edit own comment
- Can delete own comment

Admin:

- Can bypass ownership validation

Ownership validation belongs inside Service Layer.

---

# API Response Standard

Success

```json
{
  "success": true,
  "message": "Success",
  "data": {}
}
```

Error

```json
{
  "success": false,
  "message": "Validation Failed",
  "errors": []
}
```

---

# Query Engineering Rules

Support:

- Pagination
- Filtering
- Sorting
- Search

Default:

```text
page=1
pageSize=10
```

Maximum:

```text
pageSize=100
```

Use DTO Projection.

Preferred:

```csharp
.Select(...)
```

Avoid loading unnecessary columns.

Use:

```csharp
.AsNoTracking()
```

for read-only queries.

---

# Validation Rules

Use FluentValidation.

Every request DTO must have validation.

Examples:

- Email required
- Password minimum 8 chars
- Article title required
- Content required

---

# Logging Rules

Use structured logging.

Good:

```csharp
_logger.LogInformation(
    "Article {ArticleId} published by {UserId}",
    articleId,
    userId);
```

Never log:

- Password
- JWT Token
- Secrets
- Connection Strings

---

# Error Handling Rules

Use centralized exception handling.

Preferred:

```csharp
IExceptionHandler
```

or

```csharp
Exception Middleware
```

Use custom exceptions:

```csharp
NotFoundException
ValidationException
ForbiddenException
ConflictException
```

---

# Coravel Rules

Scheduled Jobs:

- ScheduledPublishJob
- WeeklyAnalyticsJob

Queued Jobs:

- NewsletterDispatchJob

Background jobs must never block HTTP response.

---

# Performance Rules

Use:

```csharp
async/await
```

Use:

```csharp
AsNoTracking()
```

Avoid:

```csharp
N+1 Query
```

Prefer:

```csharp
Include()
```

for required navigation loading.

---

# Definition of Done

Feature considered complete when:

- Business logic implemented
- Validation implemented
- Authorization implemented
- Ownership validation implemented
- Swagger documented
- Error handling covered
- Logging added
- Migration generated
- Build successful
- No warnings introduced
- Endpoint tested via Postman

---

# Development Principles

1. Thin Controllers
2. Fat Services
3. Repository for persistence only
4. DTO everywhere
5. Never expose Entity directly
6. Soft delete over hard delete
7. Async first
8. Secure by default
9. Consistent API responses
10. Maintainability over cleverness

```

```
