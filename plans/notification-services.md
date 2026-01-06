# Feature Implementation Plan: Notification Services

## 📋 Todo Checklist
- [x] Move and Adapt Core Interfaces & Models to `libs/ReSys.Core/Common/Notifications`
- [x] Move and Adapt Constants to `libs/ReSys.Core/Common/Notifications/Constants`
- [x] Implement Infrastructure Services (Logger-based) in `libs/ReSys.Infrastructure/Notifications`
- [x] Register Notification Services in Dependency Injection
- [x] Implement Unit Tests
- [x] Final Verification

## 🔍 Analysis & Investigation

### Codebase Structure
- **Sample Code**: `NotificationServiceSample` (Root) contains a complete but unintegrated notification implementation.
- **Core Library**: `libs/ReSys.Core/Common` contains shared capabilities like `Imaging`, `Storage`.
- **Infrastructure Library**: `libs/ReSys.Infrastructure` contains implementations like `Imaging`, `Storage`.
- **Dependencies**: `FluentEmail` is used in the sample but is **not** present in `Directory.Packages.props`. `System.Net.Mail` is obsolete.

### Current Architecture
- **Vertical Slice Architecture** for Features.
- **Clean Architecture** for shared services (Core Interfaces -> Infrastructure Implementation).
- **Result Pattern**: Uses `ErrorOr` for error handling.

### Dependencies & Integration Points
- `ReSys.Core`: Will host the Interfaces (`INotificationService`) and Models (`NotificationData`).
- `ReSys.Infrastructure`: Will host the Implementation (`NotificationService`, `EmailSenderService`, `SmsSenderService`).
- **Missing Dependency**: `FluentEmail` is missing. We will use a **Logger-based implementation** for `EmailSenderService` and `SmsSenderService` to ensure the structure is correct without adding unverified dependencies. The architecture will allow swapping these for real implementations (e.g., SendGrid, Twilio, or FluentEmail) later.

### Considerations & Challenges
- **Namespace Updates**: The sample uses `ReSys.Core.Feature.Common.Notification`. We must update this to `ReSys.Core.Common.Notifications` to match the project convention.
- **Manual Validation**: The sample uses a manual `Validate()` method in models. We will preserve this pattern as it is self-contained, rather than introducing `FluentValidation` validators which are typically used for MediatR requests.
- **Sample Code Retention**: The original `NotificationServiceSample` folder will be left as-is for reference unless explicitly asked to remove.

## 📝 Implementation Plan

### Prerequisites
- None (Internal refactoring).

### Step-by-Step Implementation

1.  **Define Core Constants**
    -   **Files**: Create `libs/ReSys.Core/Common/Notifications/Constants/*.cs`
    -   **Source**: Adapt from `NotificationServiceSample/Notification/Constants/`
    -   **Action**: Move files like `Notification.UseCases.cs`, `Notification.Parameters.cs` etc. Update namespaces to `ReSys.Core.Common.Notifications.Constants`.

2.  **Define Core Models**
    -   **Files**: Create `libs/ReSys.Core/Common/Notifications/Models/*.cs`
    -   **Source**: Adapt from `NotificationServiceSample/Notification/Models/`
    -   **Action**: Create `NotificationData.cs`, `EmailNotificationData.cs`, `SmsNotificationData.cs` and their Mapper/Error classes. Update namespaces to `ReSys.Core.Common.Notifications.Models`. Ensure usage of `ErrorOr`.

3.  **Define Core Interfaces**
    -   **Files**: Create `libs/ReSys.Core/Common/Notifications/Interfaces/*.cs`
    -   **Source**: Adapt from `NotificationServiceSample/Notification/Interfaces/`
    -   **Action**: Create `INotificationService.cs`, `IEmailSenderService.cs`, `ISmsSenderService.cs`. Update namespaces to `ReSys.Core.Common.Notifications.Interfaces`.

4.  **Implement Infrastructure Options**
    -   **Files**: Create `libs/ReSys.Infrastructure/Notifications/Options/NotificationOptions.cs`
    -   **Action**: Create `SmtpOptions` and `SmsOptions` classes (adapted from sample) to support future configuration.

5.  **Implement Sender Services (Logger)**
    -   **Files**:
        -   `libs/ReSys.Infrastructure/Notifications/Services/LoggerEmailSenderService.cs`
        -   `libs/ReSys.Infrastructure/Notifications/Services/LoggerSmsSenderService.cs`
    -   **Action**: Implement `IEmailSenderService` and `ISmsSenderService` that strictly **log** the notification details (To, Subject, Content) using `ILogger`. This avoids adding external dependencies like `FluentEmail` while verifying the pipeline.

6.  **Implement Notification Service**
    -   **Files**: `libs/ReSys.Infrastructure/Notifications/NotificationService.cs`
    -   **Source**: Adapt from `NotificationServiceSample/Notification/Interfaces/Notifications/Services/Notification.Service.cs`
    -   **Action**: Implement the orchestration logic (Validation -> Dispatch). Inject `IEmailSenderService` and `ISmsSenderService`. Remove `IServiceScopeFactory` usage if not strictly needed (or keep if dependencies require it, but Logger services won't).

7.  **Register Services**
    -   **Files**: `libs/ReSys.Infrastructure/Notifications/DependencyInjection.cs`
    -   **Action**: Create an extension method `AddNotifications(this IServiceCollection services, IConfiguration configuration)`. Register services (likely Scoped or Transient).
    -   **Update**: Modify `libs/ReSys.Infrastructure/DependencyInjection.cs` to call `.AddNotifications(configuration)`.

### Testing Strategy
-   **Unit Tests**: Create `tests/ReSys.Infrastructure.UnitTests/Notifications/NotificationServiceTests.cs`.
    -   Test `AddNotificationAsync` with various UseCases (Email, SMS).
    -   Verify that the correct Sender Service is called (using NSubstitute).
    -   Verify validation logic (e.g., missing receivers returns error).

## 🎯 Success Criteria
-   `INotificationService` is available for injection in other services.
-   Calling `AddNotificationAsync` results in a successful log message indicating the "Email Sent" (simulated).
-   All Unit Tests pass.
-   Project builds without errors.
