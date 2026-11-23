# Entity Framework Core

- Use EF Core 8 features when relevant.
- Use AsNoTracking() for read-only queries.
- Prevent N+1 queries.
- Prefer projections over loading full entities.
- All database operations must support CancellationToken.
- Use async methods only.
- Avoid unnecessary ToList() calls.
