# Feature Implementation Plan: Notification Models Refinement

## 📋 Todo Checklist
- [ ] Refactor Constants to remove partial classes and use logical grouping
- [ ] Refactor Core Models to `public sealed record` with structured Context
- [ ] Update `INotificationService` interface with intent-based methods
- [ ] Implement `NotificationContext` for type-safe template data
- [ ] Update `EmailSenderService` and `SmsSenderService` implementations
- [ ] Update `NotificationService` (Facade) to handle structured context mapping
- [ ] Update Validators to match new record structures
- [ ] Final Build and Verification

## 🔍 Analysis & Investigation

### Current Codebase Discoveries
- The current implementation uses `partial class NotificationConstants` spread across 6 files, which makes it hard to track.
- `NotificationData` is a class with many nullable fields (HtmlContent, SenderNumber, etc.), leading to "blind" usage where developers aren't sure which fields to fill.
- Mapping uses manual `.Replace("{Parameter}", value)` which is error-prone.

### Proposed Architecture: "Smart Context Command"
- **Records**: Use `public sealed record` for all DTOs to ensure thread safety and immutability.
- **Contextual Payload**: Instead of a dictionary of parameters, we use a structured `NotificationContext` that contains standard e-commerce fields (OrderId, Amount, UserName).
- **Strongly Typed Intent**: Split the "God Model" into specific messages for Email and SMS.

## 📝 Implementation Plan

### Step 1: Refactor Constants
- **Files**: `libs/ReSys.Core/Common/Notifications/Constants/*.cs`
- **Changes**: 
    - Keep `UseCase`, `PriorityLevel`, and `SendMethod` as standard Enums.
    - Combine `NotificationConstants.Templates` into a single lookup class or keep in `NotificationConstants.UseCases.cs` but simplify the structure.
    - Remove `NotificationConstants.Parameter` enum in favor of properties in `NotificationContext`.

### Step 2: Define Structured Models
- **Files**: `libs/ReSys.Core/Common/Notifications/Models/`
- **New Structures**:
    - `public sealed record NotificationMetadata(PriorityLevel Priority, string Language, string CreatedBy);`
    - `public sealed record NotificationContext(...)`: Contains properties like `UserName`, `OrderId`, `OtpCode`, `Link`, etc.
    - `public sealed record EmailMessage(...)`: Strictly for email (To, Subject, Body, HtmlBody, Context, Attachments).
    - `public sealed record SmsMessage(...)`: Strictly for SMS (To, Message, SenderNumber).

### Step 3: Update Interfaces
- **File**: `libs/ReSys.Core/Common/Notifications/Interfaces/INotificationService.cs`
- **Changes**: Update to:
    ```csharp
    Task<ErrorOr<Success>> SendEmailAsync(EmailMessage message, CancellationToken ct);
    Task<ErrorOr<Success>> SendSmsAsync(SmsMessage message, CancellationToken ct);
    Task<ErrorOr<Success>> NotifyAsync(UseCase useCase, string recipient, NotificationContext context, CancellationToken ct);
    ```

### Step 4: Implement High-Level Orchestration
- **File**: `libs/ReSys.Infrastructure/Notifications/Services/NotificationService.cs`
- **Action**: Implement `NotifyAsync`. It will look up the Template by `UseCase`, merge the `NotificationContext` into the template strings, and then call `SendEmailAsync` or `SendSmsAsync`.

### Step 5: Update Mapping Logic
- **File**: `libs/ReSys.Core/Common/Notifications/Models/NotificationData.Mapper.cs`
- **Action**: Update the mapper to use Reflection or a simple property-to-string mapping to fill template placeholders from the `NotificationContext` record.

### Step 6: Update Validators
- **File**: `libs/ReSys.Core/Common/Notifications/Validators/*.cs`
- **Action**: Update `AbstractValidator` implementations to work with the new `record` types.

### Testing Strategy
- Update existing unit tests to use the new `record` syntax.
- Verify that `NotificationContext` correctly populates placeholders in templates.

## 🎯 Success Criteria
- No "God Objects" in the notification slice.
- Full immutability for all data passing through the system.
- Compile-time safety for common notification parameters (e.g. `context.OrderId` instead of `values[Parameter.OrderId]`).
- Clear, descriptive folder structure.
