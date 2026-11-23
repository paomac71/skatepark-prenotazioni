# Security

- Never hardcode passwords, API keys or connection strings.
- Store secrets using User Secrets, Azure Key Vault or environment variables.
- Validate and sanitize all external input.
- Apply authentication and authorization to protected endpoints.
- Prefer JWT Bearer authentication for APIs.
- Never return stack traces to clients.
- Log security events without exposing sensitive data.
- Use HTTPS only.
- Follow OWASP Top 10 recommendations.
- Use parameterized queries and EF Core to prevent SQL Injection.
