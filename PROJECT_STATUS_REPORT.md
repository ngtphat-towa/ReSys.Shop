# ReSys.Shop Backend: Ordering & Payment System Status Report

## 1. Executive Summary
The backend for ReSys.Shop's Ordering and Payment vertical is now **Feature Complete** and **Architecturally Robust**. We have successfully implemented a secure, audit-ready e-commerce engine that supports Guest Carts, Inventory Reservation (to prevent overselling), and Stripe Payment Integration.

The system strictly adheres to the **Modular Monolith** architecture, using **CQRS** for separation of concerns and **Domain-Driven Design** for business logic integrity.

---

## 2. Core Domain Architecture

### A. The Order Aggregate
The `Order` entity is the heart of the system. It enforces a strict state machine to prevent invalid operations.

*   **States**: `Cart` → `Address` → `Delivery` → `Payment` → `Confirm` → `Complete`.
*   **Identity**: Supports both **Authenticated Users** (`UserId`) and **Guest Visitors** (`SessionId`).
*   **Financials**: Auto-calculates `TotalCents` based on Line Items + Shipping - Discounts.
*   **Inventory**: Tracks physical items via `InventoryUnit` (granular tracking).

### B. The Payment System
We built a flexible, provider-agnostic payment infrastructure.

*   **Configuration**: Payment Methods (e.g., Stripe) store their API keys in `PrivateMetadata` in the database. This allows multi-tenant configuration without redeployment.
*   **Processor**: `StripeProcessor` handles the actual communication with Stripe.
*   **Security**: Webhook signatures are verified to prevent spoofing.

---

## 3. Data Flow & User Journey

Here is the exact lifecycle of an order in the current `src` codebase:

### Phase 1: Shopping (Guest or User)
1.  **Add Item**: `POST /api/storefront/cart/items`
    *   **Handler**: `AddToCart.cs`
    *   **Logic**: Finds existing `Cart` by User OR Session. Adds `LineItem` with price snapshot.
2.  **View Cart**: `GET /api/storefront/cart`
    *   **Handler**: `GetCart.cs`
    *   **Logic**: Returns DTO with calculated totals and product images.

### Phase 2: Checkout (User Only)
3.  **Login/Merge**: `POST /api/storefront/cart/merge`
    *   **Handler**: `MergeCart.cs`
    *   **Logic**: Moves items from Guest Cart to User Cart.
4.  **Set Addresses**: `POST /api/storefront/checkout/addresses`
    *   **Logic**: Validates addresses belong to user. Transitions state to `Address`.
5.  **Select Shipping**: `POST /api/storefront/checkout/shipping-method`
    *   **Logic**: Updates `ShipmentTotalCents`. Transitions state to `Delivery`.

### Phase 3: Payment & Fulfillment
6.  **Place Order**: `POST /api/storefront/checkout/place-order`
    *   **Handler**: `PlaceOrder.cs`
    *   **Critical Guard**: Calls `InventoryReservationService` to lock stock. **If fails, abort.**
    *   **Action**: Calls `StripeProcessor` to create `PaymentIntent`.
    *   **Result**: Returns `clientSecret` to frontend.
7.  **Async Confirmation**: `POST /api/webhooks/stripe`
    *   **Trigger**: Stripe confirms payment success.
    *   **Handler**: `WebhookController` -> `StripeProcessor`.
    *   **Event**: Publishes `ExternalPaymentCaptured`.
8.  **Completion**: `PaymentCapturedHandler`
    *   **Action**: Updates Order Payment status to `Captured`.
    *   **Result**: If fully paid, Order transitions to `Complete`.
    *   **Notification**: Sends "Order Confirmation" email via `OrderCompletedEmailHandler`.

---

## 4. Key Technical Achievements

### ✅ Inventory Concurrency Control
We implemented the **"Soft Reserve" pattern**.
*   **Service**: `InventoryReservationService`
*   **Logic**: Checks stock *before* payment. Locks it in the DB transaction.
*   **Safety**: If payment fails, the reservation is released.

### ✅ Secure Configuration API
We built a specialized Admin endpoint for managing secrets.
*   **Endpoint**: `PUT /api/admin/settings/payment-methods/{id}/config`
*   **Logic**: Writes to `PrivateMetadata`. Does *not* expose secrets in GET requests.

### ✅ Unified API Responses
All endpoints now use `ApiResponse<T>` via the `ToTypedApiResponse()` extension, ensuring a consistent contract for the frontend.

---

## 5. Directory & File Reference

| Component | Path |
| :--- | :--- |
| **Order Entity** | `src/libs/ReSys.Core/Domain/Ordering/Order.cs` |
| **Cart Handlers** | `src/libs/ReSys.Core/Features/Storefront/Cart/` |
| **Checkout Handlers** | `src/libs/ReSys.Core/Features/Storefront/Checkout/` |
| **Payment Logic** | `src/libs/ReSys.Infrastructure/Payments/Gateways/StripeProcessor.cs` |
| **Inventory Service** | `src/libs/ReSys.Infrastructure/Inventories/Services/InventoryReservationService.cs` |
| **Endpoints** | `src/services/ReSys.Api/Features/Storefront/` |

---

## 6. Conclusion
The backend is **ready**. It handles the complex orchestration of money, inventory, and data consistency required for a real-world shop. The separation of concerns is clean, and the critical business risks (overselling, lost payments) have been mitigated.
