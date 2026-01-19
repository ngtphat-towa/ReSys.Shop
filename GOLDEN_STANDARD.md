# ReSys.Shop: The Golden Standard

This document defines the mandatory architectural patterns and implementation standards for the ReSys.Shop project. Every feature slice must adhere to these pillars to ensure system integrity, financial auditability, and logistical precision.

---

## 1. Domain-Driven Integrity (The Core)

### Aggregate Roots & Entities
- **Encapsulation**: State changes *must* happen through explicit domain methods (e.g., `.AdjustStock()`, `.Ship()`). While using `private set` is highly recommended to enforce this, the primary focus is ensuring all mutations are deliberate and encapsulated within the domain.
- **Aggregate Promotion**: Use `Aggregate` base for roots that manage children or raise events. Use `Entity` for child items.
- **Invariants (Guards)**: Every domain method must validate its own rules immediately.
    - *Example:* `if (quantity == 0) return StockItemErrors.ZeroQuantityMovement;`
- **ErrorOr Pattern**: Direct "throws" are forbidden. Every state-changing method must return `ErrorOr<T>` to provide meaningful business feedback.

### Centralized Constants & Errors
- **Constraints**: All length limits, regex patterns, and decimal precisions must reside in a `*Constraints.cs` file within the domain folder.
- **Predefined Errors**: All business failures must be defined in a `*Errors.cs` file. Hardcoded strings in handlers or domains are forbidden.
- **Precision**: 
    - Financial values (Price, Cost): `(18, 2)`.
    - Physical measurements (Weight, Dimensions): `(18, 4)`.

---

## 2. The Persistence Contract (Infrastructure)

### Explicit Context Operations
To ensure the EF Core `ChangeTracker` remains predictable and to prevent Foreign Key exceptions:
- **Handlers must call explicit methods**: Always use `context.Set<T>().Add()`, `.Update()`, or `.Remove()` before calling `SaveChangesAsync()`.
- **No "Magic" Tracking**: Do not rely on navigation property auto-saves for complex logic.

### PostgreSQL Optimization
- **Native Enums**: Map enums as PostgreSQL Native Enums via `AppDbContext`. Avoid `.HasConversion<string>()` unless specifically required for external compatibility.
- **Concurrency**: All Aggregate Roots must implement a `uint Version` property mapped to `IsRowVersion()` to prevent "Lost Updates" in high-concurrency scenarios.

---

## 3. Feature Slice Architecture (Application)

### Granular CQRS
- **Structure**: Every operation (Create, Update, Delete, GetDetail) must have its own dedicated folder.
- **Deduplication**: Use a `Common` folder within the feature slice for shared Models, Mappings, and Validators.
- **Mappings**: Use **Mapster** for DTO projections. Complex logic (like selecting a default image) should be handled in the mapping configuration, not the handler.

### Resilience & Behaviors
- **UnhandledExceptionBehavior**: Provides global `try-catch` and logging. Handlers should be kept clean of manual `try-catch` blocks.
- **AuditableEntityInterceptor**: Automatically manages `CreatedAt`, `CreatedBy`, `UpdatedAt`, and `UpdatedBy`. Manual timestamp assignment is forbidden.

---

## 4. The Inventory Ledger (Standard for IMS)

### Immutable Audit Trail
- Every physical change to stock *must* result in a `StockMovement` record.
- **Snapshots**: Movements must record `BalanceBefore` and `BalanceAfter`.
- **Financials**: Movements must record `UnitCost` at the moment of the transaction.

### Chain of Custody
- **Fulfillment**: Items must be `Reserved` (OnHand) before they can be `Shipped`. 
- **Traceability**: Every movement requires a `Reference` (Order ID, PO Number, or Audit ID).

---

## 5. Documentation Standards

### XML Documentation
- All public classes and methods must have `<summary>` tags.
- Explain the **Business Intent** of the method, not just the technical action.

### Inline Logic Comments
- Use "Guard" comments to highlight business rules.
    - *Example:* `// Guard: Prevent deactivating the anchor location.`
- Use "Business Rule" comments for complex multi-step logic.

---

## 7. Code Maintenance & Tool Usage

### Comment Preservation
- **Non-Destructive Updates**: Every tool call (`replace`, `write_file`) MUST preserve existing XML documentation, `<summary>` tags, and inline logic comments.
- **Anti-Truncation**: NEVER truncate a file during an update. If a large file needs modification, use granular `replace` calls or ensure the full content is provided in `write_file`.
- **Self-Documenting Code**: Any new logic added must include its own "Guard" and "Business Rule" comments following the established style.

### Granular and Transparent Edits
- **Contextual Transparency**: Every code modification must clearly state the context, describe exactly what is being changed, and provide actionable details on the expected outcome.
- **Justified Modifications**: Every change must be accompanied by a clear reason "why" it is being made (e.g., to fix a specific bug, implement a feature, or adhere to an architectural standard).
- **Modification Granularity**: Changes should be kept small and focused. Avoid modifications exceeding 50 lines per operation to ensure clarity, maintainability, and ease of review.

---

## 6. Git & Commit Standards

- **Atomic Commits**: Split commits by context (Domain, Feature, Infrastructure).
- **Conventional Commits**: Use `feat:`, `fix:`, `refactor:`, `domain:`, `persistence:`, or `chore:`.
