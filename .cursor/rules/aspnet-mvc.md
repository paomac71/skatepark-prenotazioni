# ASP.NET Core MVC

## Controllers

- Controllers must remain thin.
- Controllers orchestrate requests and responses only.
- Business logic belongs to services.

Preferred:

```csharp
public async Task<IActionResult> Details(int id)
{
    var model = await productService.GetByIdAsync(id);

    return model == null
        ? NotFound()
        : View(model);
}
```

## Views

- Keep Views focused on presentation.
- Avoid business logic in Razor Views.
- Use strongly typed ViewModels.

Preferred:

```csharp
@model ProductDetailViewModel
```

Avoid:

```csharp
@model Product
```

## ViewModels

- Use dedicated ViewModels.
- One ViewModel per screen whenever possible.
- Avoid exposing EF entities to views.

## Dependency Injection

- Use constructor injection.
- Never instantiate services directly.

Preferred:

```csharp
public class ProductController(
    IProductService productService)
{
}
```

## Validation

- Use DataAnnotations and FluentValidation.
- Validate on both client and server.

Example:

```csharp
[Required]
[StringLength(100)]
public string Name { get; set; }
```

## Razor

- Prefer TagHelpers.
- Avoid duplicated markup.

Preferred:

```html
<input asp-for="Name" />
<span asp-validation-for="Name"></span>
```

## Routing

- Use attribute routing when appropriate.
- Keep route names consistent.

## Security

- Use AntiForgery protection on forms.

Preferred:

```html
@Html.AntiForgeryToken()
```

- Validate authorization on protected actions.
- Never trust user input.

## Partial Views

- Use Partial Views for reusable UI.
- Use View Components for complex reusable functionality.

## Error Handling

- Use centralized error handling.
- Return custom error pages.
- Log unexpected exceptions.

## Performance

- Avoid loading unnecessary data.
- Use paging for large collections.
- Cache data when appropriate.

## Cursor Instructions

When generating MVC code:

- Generate ViewModel classes.
- Generate Controllers separately from Services.
- Use strongly typed Views.
- Use TagHelpers.
- Follow MVC separation of concerns.
- Never place business logic in Views.