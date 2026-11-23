# Tailwind CSS

## General

- Prefer Tailwind utility classes over custom CSS.
- Avoid creating CSS files unless necessary.
- Reuse existing design patterns.
- Keep styling consistent across the application.

## Layout

- Prefer Flexbox and Grid utilities.
- Use container and max-width utilities.
- Avoid fixed widths unless required.
- Design mobile-first.

Preferred:

```html
<div class="flex flex-col gap-4 md:flex-row">
</div>
```

## Spacing

- Use Tailwind spacing scale only.
- Avoid arbitrary values whenever possible.

Preferred:

```html
p-4
px-6
gap-4
space-y-2
```

Avoid:

```html
p-[13px]
mt-[17px]
```

## Colors

- Use configured theme colors.
- Avoid hardcoded hex colors.
- Support dark mode.

Preferred:

```html
bg-primary
text-primary
```

Avoid:

```html
bg-[#1273ea]
```

## Typography

- Use Tailwind typography scale.
- Maintain visual hierarchy.

Preferred:

```html
text-sm
text-base
text-lg
text-xl
text-2xl
```

## Components

- Extract repeated UI patterns into components.
- Keep utility classes readable.
- Avoid excessively long class strings.

## Accessibility

- Include focus states.
- Ensure keyboard navigation.
- Maintain sufficient contrast ratios.

Preferred:

```html
focus:outline-none
focus:ring-2
focus:ring-primary
```

## Responsive Design

- Use responsive modifiers.

Preferred:

```html
grid-cols-1
md:grid-cols-2
lg:grid-cols-4
```

## Performance

- Remove unused classes.
- Minimize custom CSS.
- Prefer Tailwind utilities over inline styles.

## Cursor Instructions

When generating UI:

- Use Tailwind first.
- Produce responsive layouts.
- Support dark mode.
- Follow existing component conventions.
- Keep utility usage consistent.