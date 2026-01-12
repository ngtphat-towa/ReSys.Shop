# Advanced Identity & Access Management (IAM) System

## 1. Product Overview

### Core Value Proposition
A robust, fine-grained, and unified security framework for `ReSys.Shop` that manages complex user hierarchies, dynamic permissions, and automated account lifecycles. It moves beyond simple authentication into a comprehensive authorization engine that handles inherited permissions and real-time user context resolution.

### Target Audience
1.  **System Administrators**: Need to manage roles and global permission catalogs (`ClaimsMaster`).
2.  **Internal Staff**: Need varying levels of access to the Admin Portal based on specialized roles.
3.  **End Customers**: Need secure account management (email changes, credential updates) with automated notifications.

---

## 2. Functional Specifications

### 2.1 Full Account Management Flow
- **Credential Management**: Secure password updates and multi-factor authentication (MFA) setup.
- **Verified Email Changes**: A two-step verification process where a token is sent to both the old and new email addresses using the existing `Notifications` module.
- **Automated Lifecycle**: Integration with the `Notifications` system for welcome emails, password reset requests, and security alerts using predefined templates.

### 2.2 Advanced Permission Model (RBAC + PBAC)
- **Claims Master Catalog**: A centralized table defining all available system permissions (e.g., `Permissions.Orders.Delete`).
- **Inheritance & Union**: 
    - **Roles**: Contain multiple claims. Users assigned to roles inherit these claims.
    - **Direct User Claims**: Specific "Override" permissions assigned directly to a user.
    - **Effective Permissions**: The union of all role-inherited claims and direct user claims.
- **Dynamic Membership**: Support for users having multiple roles simultaneously.

### 2.3 Unified Authorization Engine
- **Granular Endpoints**: Ability to secure endpoints using:
    - **RBAC**: `[Authorize(Roles = "Admin")]`
    - **PBAC**: `[RequirePermission("orders.delete")]` (Must possess at least this specific claim).
    - **Policy-Based**: Complex requirements combining roles and claims.
- **Non-Overriding Tokens**: Standard OIDC tokens remain clean; extended attributes are handled via the User Context service or backend claims transformation.

### 2.4 User Context Resolution
- **Context Service**: An `IUserContext` service available in the `Core` layer to provide the current user's ID, Roles, and effective Permissions for data-level filtering.

---

## 3. Technical Specifications

### Architecture Overview
The system follows the **Clean Architecture** and **CQRS** patterns already established in the project. The IAM system will be split between `ReSys.Identity` (Provider) and `ReSys.Api` (Resource Server).

### Data Schema (EF Core)
- **ClaimsMaster**: Metadata for all system permissions.
- **AspNetRoles / AspNetRoleClaims**: Standard Identity tables linked to `ClaimsMaster`.
- **AspNetUsers / AspNetUserClaims**: Standard Identity tables for direct overrides.
- **UserRoles**: Many-to-many relationship.

### Technology Stack
- **Backend**: .NET 9, ASP.NET Core Identity, OpenIddict.
- **CQRS**: MediatR for all account and management operations.
- **Validation**: FluentValidation for complex business rules (e.g., password strength, unique email).
- **Communication**: Shared `AuthConstants` for role and claim names.

### Key Technical Decisions
- **Custom Authorization Policy Provider**: To avoid manual policy registration for every permission, a dynamic provider will parse the `RequirePermission` attributes.
- **Claims Transformation**: Use `IClaimsTransformation` or a scoped `UserContext` service to calculate the "Union" of permissions on every request without bloating the JWT.

---

## 4. MVP Scope

### Minimum Viable Product Features
- `ClaimsMaster` CRUD management in Admin Portal.
- Role management with claim assignment.
- User registration and role assignment.
- `IUserContext` implementation in the API.
- Secured "Example" mutation endpoints using the new permission engine.

### Success Metrics
- 100% test coverage for permission union logic.
- Zero hardcoded role/claim strings in the presentation layer (using `AuthConstants`).
- Successful integration of Email change flow with the Notifications service.

---

## 5. Next Steps

1.  **Database Migration**: Extend `AppIdentityDbContext` to include the `ClaimsMaster` and seed it with initial permissions.
2.  **Core Interfaces**: Define `IUserContext` and permission attributes in `ReSys.Core`.
3.  **Account Logic**: Implement MediatR commands for Email Change and Credential management in `ReSys.Identity`.
4.  **Auth Handler**: Implement the `PermissionAuthorizationHandler` in `ReSys.Api`.
