# Feature Implementation Plan: Notification Services Refinement & Optimization

## 📋 Todo Checklist
- [x] Refactor `NotificationMessage` as the Central Unified Model ✅ Implemented (Constructor hidden)
- [x] Refactor `NotificationContext` for Type-Safety and Performance ✅ Implemented
- [x] Optimize `NotificationMapper` Template Filling logic (Regex) ✅ Implemented
- [x] Refactor `NotificationContextBuilder` into a unified `NotificationMessageBuilder` ✅ Implemented
- [x] Update `INotificationService` to a single `SendAsync` method ✅ Implemented
- [x] Refine `EmailSenderService` and `SmsSenderService` to accept unified parameters ✅ Implemented
- [x] Add Validation (FluentValidation) to Notification Models ✅ Implemented
- [x] Add Comprehensive Unit Tests ✅ Implemented
- [x] Final Build and Verification ✅ Implemented

## 🔍 Analysis & Investigation

### Current State Discoveries
- **NotificationContext**: Replaced raw dictionary with strongly-typed properties and a fallback dictionary.
- **Unified Model**: `NotificationMessage` is now the primary carrier. Constructor is `internal` to force usage of the builder.
- **Template Filling**: Migrated from O(N*M) loop to O(N) `Regex.Replace` with compiled patterns.
- **Service API**: Simplified to a single `SendAsync` method.
- **Validation**: Full validation pipeline using `FluentValidation` integrated into `NotificationService`.

## 📝 Implementation Notes

### Unified Model Flow
Calls enter via `SendAsync`, which validates the message using `NotificationMessageValidator` (including deep validation of recipients and attachments based on the UseCase).

### Optimized Mapping
`NotificationMapper.FillTemplate` uses `PlaceholderRegex.Replace` with a `MatchEvaluator` for O(N) performance.

### Success Criteria Verified
- [x] `NotificationMessage` used as internal unified model.
- [x] `NotificationContext` has type-safe properties.
- [x] Regex-based filling implemented and cached.
- [x] `INotificationService` reduced to a single `SendAsync` method.
- [x] Comprehensive tests for Builders, Models, and Validators.
- [x] Solution builds successfully.
