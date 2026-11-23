# API Guidelines

- Controllers must be thin.
- Return appropriate HTTP status codes.
- Use ProblemDetails for errors.
- Use DTOs for API contracts.
- Never expose entities directly.
- Validate input using FluentValidation.
- Use ILogger<T> for logging.
