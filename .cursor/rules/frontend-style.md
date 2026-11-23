# Frontend Style Guide

## General Principles

- Write clean, readable and maintainable code.
- Prefer simplicity over clever solutions.
- Reuse existing components before creating new ones.
- Avoid code duplication.

## HTML

- Use semantic HTML elements whenever possible.
- Ensure accessibility (a11y) by default.
- Every form control must have a label.
- Use meaningful names for ids and classes.

## CSS

- Prefer Flexbox and CSS Grid.
- Avoid inline styles.
- Use a consistent spacing scale.
- Minimize custom CSS when framework utilities are available.
- Support responsive layouts from mobile to desktop.

## JavaScript / TypeScript

- Prefer TypeScript over JavaScript.
- Use strict typing.
- Avoid any unless absolutely necessary.
- Prefer const over let.
- Avoid deeply nested logic.
- Keep functions small and focused.

## Components

- Components should have a single responsibility.
- Keep components reusable.
- Extract repeated UI into shared components.
- Prefer composition over inheritance.

## State Management

- Keep state as local as possible.
- Avoid unnecessary global state.
- Do not duplicate state.

## Forms

- Validate on both client and server.
- Show clear validation messages.
- Disable submit buttons during processing.

## Accessibility

- Keyboard navigation must work.
- Use aria attributes only when necessary.
- Ensure sufficient color contrast.
- All images require alt text.

## Performance

- Avoid unnecessary re-renders.
- Lazy load heavy components.
- Optimize images and assets.
- Avoid unnecessary API calls.

## UI/UX

- Maintain a consistent visual style.
- Use loading indicators for async actions.
- Display user-friendly error messages.
- Prefer clarity over visual complexity.

## Cursor Instructions

When generating frontend code:

- Return complete components.
- Follow existing project structure.
- Reuse existing styles and components.
- Ensure responsive design.
- Ensure accessibility compliance.
- Avoid placeholder implementations.