# Identity Management Proposal for ReSys.Shop

## 1. Product Overview

### Core Value Proposition
Secure, scalable, and unified identity management for `ReSys.Shop` that seamlessly handles internal administrative users (Admin Portal) and external customers (Shop Storefront).

### Target Audience
1.  **Shop Customers (B2C/B2B)**: Need low-friction sign-up, social login, and fast access.
2.  **Shop Administrators (Internal)**: Need high-security access (RBAC), potential MFA, and strict session control.

---

## 2. Functional Specifications

### 2.1 Unified Authentication
- Centralized login/logout capabilities.
- Support for multiple user types (Admin vs. Customer) sharing or separating logic as needed.

### 2.2 Role-Based Access Control (RBAC)
- **Admin**: Super Admin, Manager, Editor.
- **Shop**: Guest, Customer, VIP, B2B Account.

### 2.3 Token Management
- Issue Access Tokens (JWT) for API access.
- Refresh Tokens for long-lived sessions without re-login.

---

## 3. Technical Options

Here are 5 comprehensive options for implementing identity in `ReSys.Shop`.

### Option 1: Monolithic ASP.NET Core Identity (Embedded)
**Concept**: Implement standard ASP.NET Core Identity directly inside `ReSys.Api`.
**Fit**: Good for keeping it simple if `ReSys.Api` is the only backend.

*   **Architecture**: `ReSys.Api` has `AddIdentity<User, Role>()`. No separate `ReSys.Identity` service needed.
*   **Tech**: `Microsoft.AspNetCore.Identity`, `JWT Bearer`.
*   **Pros**:
    *   **Simplicity**: Zero extra infrastructure complexity.
    *   **Speed**: Fastest to implement (scaffoldable).
    *   **Cost**: Free.
*   **Cons**:
    *   **Coupling**: Auth logic tightly bound to business API.
    *   **Scaling**: Harder to scale auth independently from logic.
    *   **Frontend**: Must build all login/register UI pages manually in Vue.

### Option 2: Dedicated Identity Service (OpenIddict)
**Concept**: Build a dedicated OIDC Provider in `services/ReSys.Identity` using OpenIddict.
**Fit**: Best alignment with the current project structure (Microservices/Modular).

*   **Architecture**: `ReSys.Identity` is a standalone service that issues tokens. `ReSys.Api` just verifies them.
*   **Tech**: OpenIddict, ASP.NET Core, Entity Framework.
*   **Pros**:
    *   **Standards Compliant**: Full OIDC/OAuth2 support.
    *   **Decoupled**: `ReSys.Api` doesn't touch user passwords.
    *   **Control**: Full control over the login flow and data.
*   **Cons**:
    *   **Complexity**: High learning curve for OIDC protocols.
    *   **Maintenance**: You own the security patches.

### Option 3: External SaaS (Auth0 / Clerk / Azure AD B2C)
**Concept**: Offload all identity logic to a 3rd party provider.
**Fit**: Best if you want to avoid security maintenance and just write business code.

*   **Architecture**: Frontends talk to Auth0. `ReSys.Api` validates Auth0 JWTs.
*   **Tech**: Auth0 SDKs or OIDC libraries.
*   **Pros**:
    *   **Security**: World-class security out of the box (MFA, Social Login).
    *   **Speed**: Very fast initial setup.
    *   **Features**: User management UI provided by vendor.
*   **Cons**:
    *   **Cost**: Can get expensive as MAU (Monthly Active Users) grows.
    *   **Vendor Lock-in**: Hard to migrate away later.
    *   **Data Sovereignty**: User data lives on their servers.

### Option 4: Self-Hosted Container (Keycloak / Zitadel)
**Concept**: Run a ready-made Identity Provider container (Docker) alongside your app.
**Fit**: Best for "Enterprise" features without SaaS costs.

*   **Architecture**: A `Keycloak` container running in Docker. `ReSys.Api` validates JWTs.
*   **Tech**: Java (Keycloak) or Go (Zitadel).
*   **Pros**:
    *   **Feature Rich**: Admin console, social auth, federation, MFA included.
    *   **Free**: Open source (operational cost only).
*   **Cons**:
    *   **Resource Heavy**: Keycloak is memory hungry.
    *   **Ops Complexity**: You must manage updates, database, and availability of another complex service.

### Option 5: Hybrid Strategy (Best of Both Worlds)
**Concept**: Split the audience.
**Fit**: Best for diverse security requirements.

*   **Architecture**:
    *   **Admins**: Login via Azure Entra ID (Corporate Office 365 accounts).
    *   **Customers**: Login via Local ASP.NET Identity or Socials (Google/Facebook).
*   **Tech**: Microsoft.Identity.Web (Admin) + ASP.NET Identity (Shop).
*   **Pros**:
    *   **Security**: Admins are secured by corporate policy (MFA, etc).
    *   **UX**: Customers get a simple, fast experience.
*   **Cons**:
    *   **Code Complexity**: Handling two different auth pipelines in the API.

---

## 4. Deep Dive: Recommendation (Option 2)

**Why OpenIddict?**
OpenIddict allows us to turn our empty `services/ReSys.Identity` project into a full-blown "Auth Server" (like our own private version of Auth0). This matches the project's ambition (Microservices) and provides long-term flexibility without recurring SaaS costs.

### 4.1 Architecture & Data Flow

There are 3 main players in this architecture:

1.  **The Identity Provider (IDP)**: `ReSys.Identity`
    *   **Role**: The "Notary". It holds the user database (Postgres), checks passwords, and issues "stamped" tokens (JWTs).
    *   **Database**: Has its own tables for `AspNetUsers`, `AspNetRoles`, and OpenIddict tables (Applications, Scopes, Tokens).

2.  **The Client Apps**: `ReSys.Admin` and `ReSys.Shop` (Vue)
    *   **Role**: The "Requestors". They don't see passwords. They just ask the IDP: "Please sign this user in."
    *   **Protocol**: They use **OAuth 2.0 Authorization Code Flow with PKCE**. This is the standard for Single Page Apps (SPAs) to ensure security in the browser.

3.  **The Resource Server**: `ReSys.Api`
    *   **Role**: The "Gatekeeper". It trusts the IDP. When a request comes in with a token, it checks the digital signature. If the signature matches the IDP's, it lets the request through.

### 4.2 Detailed Implementation Roadmap

#### Phase 1: Service Foundation (Infrastructure)
*Goal: Get `ReSys.Identity` running and capable of talking to the database.*
1.  **Packages**: Install `OpenIddict.AspNetCore`, `OpenIddict.EntityFrameworkCore`, and `Microsoft.AspNetCore.Identity.EntityFrameworkCore` into `services/ReSys.Identity`.
2.  **Database**:
    *   Create an `IdentityDbContext` in the identity project.
    *   Configure it to store standard User/Role tables AND OpenIddict tables.
    *   Run EF Migrations to create the schema in Postgres.
3.  **Configuration**:
    *   Register OpenIddict in `Program.cs`.
    *   Set up signing keys (Certificates used to sign the tokens). For dev, we can use ephemeral keys.

#### Phase 2: Client Configuration (The "Registry")
*Goal: Tell the Identity Server who is allowed to ask for tokens.*
1.  **Seeding**: On startup, we must seed the database with our "Applications".
    *   **Client 1**: `resys-shop-web` (The customer frontend).
    *   **Client 2**: `resys-admin-web` (The admin frontend).
    *   **Permissions**: Define what they can do (e.g., "login", "refresh token").
    *   **Redirect URIs**: Whitelist the URLs (e.g., `http://localhost:5173/callback`).

#### Phase 3: The API Connection
*Goal: Teach `ReSys.Api` how to validate tokens.*
1.  **JWT Bearer**: Configure `ReSys.Api` to use `JwtBearer` authentication.
2.  **Authority**: Point the `Authority` URL to `https://localhost:7000` (or wherever `ReSys.Identity` runs).
3.  **Audience**: Ensure the API expects tokens meant for "resource_server".

#### Phase 4: Frontend Integration (Vue)
*Goal: Allow users to click "Login".*
1.  **OIDC Client**: Install a library like `oidc-client-ts` in the Vue apps.
2.  **Login Flow**:
    *   User clicks "Login".
    *   Vue redirects browser to `ReSys.Identity/connect/authorize`.
    *   `ReSys.Identity` shows a Login Page (Razor Page).
    *   User authenticates.
    *   `ReSys.Identity` redirects back to Vue with a "code".
    *   Vue swaps "code" for "Access Token" + "Refresh Token".
3.  **API Calls**: Vue attaches the Access Token to every Axios request header (`Authorization: Bearer <token>`).

### 4.3 Summary of Pros/Cons for this Approach

| Feature | Details |
| :--- | :--- |
| **Security** | **High**. Passwords never leave the Identity service. Tokens are short-lived. |
| **UX** | **Seamless**. Supports "Single Sign-On" (SSO). Login once, access all apps. |
| **Effort** | **Medium-High**. Requires setting up the server, views (Login/Register HTML), and client configs. |
| **Cost** | **Low**. Hosting is part of your existing cluster. No per-user fees. |

---

## 5. Next Steps
1.  **Confirmation**: If this detailed roadmap looks correct, I will proceed with **Phase 1**.
2.  **Action**: I will scaffold the `ReSys.Identity` project structure and add the necessary NuGet packages.