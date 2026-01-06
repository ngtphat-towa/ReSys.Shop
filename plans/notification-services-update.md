# Feature Implementation Plan: Notification Services Update

## 📋 Todo Checklist
- [x] Add Package References to `libs/ReSys.Infrastructure/ReSys.Infrastructure.csproj`
- [x] Create `LogTemplates` in `libs/ReSys.Core/Common/Constants`
- [x] Implement `EmailSenderService` using `FluentEmail`
- [x] Implement `SinchSmsSenderService` using `Sinch` SDK
- [x] Update `DependencyInjection` to register proper services
- [x] Remove `LoggerEmailSenderService` and `LoggerSmsSenderService`
- [x] Update `NotificationService` to use Validators
- [x] Verify Tests

## 🔍 Analysis & Investigation

### Dependencies
- **Confirmed**: `FluentEmail` (Core, Smtp, SendGrid) and `Sinch` are present in `Directory.Packages.props`.
- **Action**: Must add `<PackageReference>` to `libs/ReSys.Infrastructure/ReSys.Infrastructure.csproj` to make them available in the project.

### Architecture
- **Sender Services**: Will replace temporary "Logger" implementations with robust SDK-based implementations matching the samples.
- **Validation**: Will use `FluentValidation` (already implemented in Core) injected into the Service, instead of manual `Validate()` calls.
- **Configuration**: `SmtpOptions` and `SmsOptions` are already implemented but will be refined to match sample usage.

## 📝 Implementation Plan

### Step 1: Update Infrastructure Project
- **File**: `libs/ReSys.Infrastructure/ReSys.Infrastructure.csproj`
- **Action**: Add references for:
    - `FluentEmail.Core`
    - `FluentEmail.Smtp`
    - `FluentEmail.SendGrid`
    - `Sinch`
    - `Ardalis.GuardClauses`

### Step 2: Define LogTemplates
- **File**: `libs/ReSys.Core/Common/Constants/LogTemplates.cs`
- **Action**: Create class with standard log message templates (e.g., `ServiceRegistered`, `ExternalCallStarted`) as used in the sample.

### Step 3: Implement Email Sender
- **File**: `libs/ReSys.Infrastructure/Notifications/Services/EmailSenderService.cs`
- **Source**: Adapt `NotificationServiceSample/Notifications/Emails/Email.StmpService.cs`
- **Action**: Implement using `IFluentEmail`.

### Step 4: Implement SMS Sender
- **File**: `libs/ReSys.Infrastructure/Notifications/Services/SinchSmsSenderService.cs`
- **Source**: Adapt `NotificationServiceSample/Notifications/SMS/Sms.SinchService.cs`
- **Action**: Implement using `ISinchClient`.

### Step 5: Update Registration
- **File**: `libs/ReSys.Infrastructure/Notifications/DependencyInjection.cs`
- **Source**: Adapt logic from `Email.Registeration.cs` and `Sms.Registeration.cs`.
- **Action**:
    - Configure `FluentEmail` (Smtp/SendGrid based on options).
    - Configure `SinchClient`.
    - Register `EmailSenderService` and `SinchSmsSenderService`.
    - Handle "Disabled" state (register `EmptySenderService` or similar, or just log warning and no-op). I'll implement `EmptyEmailSenderService` and `EmptySmsSenderService` for completeness as per sample.

### Step 6: Implement Empty Senders (Fallback)
- **Files**:
    - `libs/ReSys.Infrastructure/Notifications/Services/EmptyEmailSenderService.cs`
    - `libs/ReSys.Infrastructure/Notifications/Services/EmptySmsSenderService.cs`
- **Action**: Implement simple error-returning services for when features are disabled.

### Step 7: Update Notification Service
- **File**: `libs/ReSys.Infrastructure/Notifications/Services/NotificationService.cs`
- **Action**:
    - Inject `IValidator<NotificationData>`.
    - Perform validation using the validator.
    - Dispatch to senders.

### Step 8: Cleanup
- **Action**: Delete `LoggerEmailSenderService.cs`, `LoggerSmsSenderService.cs`.

### Testing Strategy
-   Update `NotificationServiceTests.cs` to mock `IValidator` and `IFluentEmail`/`ISinchClient` (indirectly via the Sender service interfaces which are already mocked).
-   Ensure build succeeds.

## 🎯 Success Criteria
-   Project compiles with new dependencies.
-   `EmailSenderService` uses `FluentEmail`.
-   `SinchSmsSenderService` uses `Sinch`.
-   Validation is performed via `FluentValidation`.
