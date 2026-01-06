# Feature Implementation Plan: Notification Builders

This plan outlines the implementation of fluent, type-safe builders for the refined notification system, adapting the patterns from the samples to work with `sealed records` and `NotificationContext`.

## 📋 Todo Checklist
- [ ] Implement `NotificationContextBuilder` for flexible parameter management
- [ ] Implement `EmailMessageBuilder` for fluent email construction
- [ ] Implement `SmsMessageBuilder` for fluent SMS construction
- [ ] Add Unit Tests for all builders
- [ ] Final Review and Verification

## 🔍 Analysis & Investigation

### Current Codebase Discoveries
- **Sample Builder**: The `NotificationDataBuilder` in samples uses extension methods on `ErrorOr<NotificationData>`, which is a procedural construction pattern.
- **Refined Models**: We have `EmailMessage`, `SmsMessage`, and `NotificationContext` as immutable or semi-immutable types.
- **Goal**: Provide a "Pro" developer experience where complex notifications can be built using a readable fluent API.

### Proposed Architecture: "Fluent Message Composition"
- **Context Builder**: Simplifies populating the `Values` dictionary in `NotificationContext`.
- **Message Builders**: Handle the boilerplate of recipients, metadata, and optional fields.
- **Consistency**: Builders will follow the project naming convention `FeatureNameBuilder.cs`.

## 📝 Implementation Plan

### Step 1: Create NotificationContextBuilder
- **File**: `libs/ReSys.Core/Common/Notifications/Builders/NotificationContextBuilder.cs`
- **Action**: Implement a class that provides methods like `WithParameter(Parameter, value)`, `WithUser(name, email)`, etc.
- **Example Usage**:
  ```csharp
  var context = new NotificationContextBuilder()
      .WithParameter(Parameter.OrderId, "123")
      .WithParameter(Parameter.UserFirstName, "John")
      .Build();
  ```

### Step 2: Create EmailMessageBuilder
- **File**: `libs/ReSys.Core/Common/Notifications/Builders/EmailMessageBuilder.cs`
- **Action**: Implement a fluent builder for `EmailMessage`.
- **Methods**: `To(recipient)`, `Cc(recipient)`, `Subject(text)`, `Body(content)`, `WithMetadata(metadata)`, `Build()`.

### Step 3: Create SmsMessageBuilder
- **File**: `libs/ReSys.Core/Common/Notifications/Builders/SmsMessageBuilder.cs`
- **Action**: Implement a fluent builder for `SmsMessage`.
- **Methods**: `To(recipient)`, `From(number)`, `Body(text)`, `Build()`.

### Step 4: Unit Tests
- **Files**:
    - `tests/ReSys.Core.UnitTests/Common/Notifications/Builders/NotificationContextBuilderTests.cs`
    - `tests/ReSys.Core.UnitTests/Common/Notifications/Builders/EmailMessageBuilderTests.cs`
    - `tests/ReSys.Core.UnitTests/Common/Notifications/Builders/SmsMessageBuilderTests.cs`
- **Action**: Verify that builders correctly populate all properties and handle collection merging (e.g., multiple recipients).

## 🎯 Success Criteria
- Builders provide a fluent interface for all notification types.
- Builders are strictly typed and prevent common mistakes (e.g. missing recipients).
- Builders correctly produce the refined `record` types.
- All new tests pass.
