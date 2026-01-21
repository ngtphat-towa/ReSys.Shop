# ReSys.Shop Ordering System Analysis (`src`)

## 1. Executive Summary
The **ReSys.Shop Ordering System** (in `src`) is a production-ready, feature-complete implementation of an e-commerce checkout engine. It is built on **Clean Architecture** principles, using **CQRS** (Command Query Responsibility Segregation) to separate business logic from infrastructure concerns.

**Key Strengths:**
*   **Safety**: Uses "Soft Reserve" inventory locking to prevent overselling.
*   **Security**: Enforces strict authentication for payments; supports anonymous shopping via secure Session IDs.
*   **Reliability**: Uses Webhooks for asynchronous payment confirmation (Resilient to browser closures).
*   **Auditability**: Every state change, price override, and transaction is immutable and logged.

---

## 2. Core Architecture

### A. Domain Model (`ReSys.Core.Domain.Ordering`)
The `Order` entity is the **Aggregate Root**. It strictly controls the lifecycle of a purchase.

*   **State Machine**:
    1.  `Cart`: Draft state. Items can be added/removed.
    2.  `Address`: Shipping/Billing addresses are locked.
    3.  `Delivery`: Shipping method is selected and cost calculated.
    4.  `Payment`: Payment intent is created. Inventory is reserved.
    5.  `Complete`: Payment confirmed. Order is final.
    *   *Invariant*: You cannot skip states. You cannot pay without an address.

*   **Financial Engine**:
    *   `LineItem`: Snapshots price at moment of add.
    *   `OrderAdjustment`: Handles promotions and fees.
    *   `RecalculateTotals()`: Ensures `Total = Items + Shipping + Tax - Discount` is always true.

### B. CQRS Feature Slices (`ReSys.Core.Features.Storefront`)
The application logic is vertically sliced by feature, not horizontally by layer.

*   **Cart Features**: `AddToCart`, `GetCart`, `MergeCart`, `RemovePromotion`, `ApplyPromotionCode`.
*   **Checkout Features**: `SetCheckoutAddresses`, `GetShippingMethods`, `SetShippingMethod`, `PlaceOrder`.

---

## 3. The Order Lifecycle (Data Flow)

### Phase 1: The Shopping Cart (Guest or User)
1.  **Action**: User clicks "Add to Cart".
2.  **Endpoint**: `POST /api/storefront/cart/items`
3.  **Logic**:
    *   Middleware checks `X-Session-ID`. If missing, generates one.
    *   `AddToCart` handler finds active cart by User ID or Session ID.
    *   Adds `LineItem` to Order.
    *   **Result**: Persistent cart record in DB.

### Phase 2: Checkout (User Only)
4.  **Action**: User logs in.
5.  **Endpoint**: `POST /api/storefront/cart/merge`
6.  **Logic**: Moves items from "Session Cart" to "User Cart".
7.  **Action**: User enters address.
8.  **Endpoint**: `POST /api/storefront/checkout/addresses`
    *   **Logic**: Validates addresses belong to user. Transitions state to `Address`.

### Phase 3: Financial Commitment
9.  **Action**: User clicks "Place Order".
10. **Endpoint**: `POST /api/storefront/checkout/place-order`
11. **Logic (Critical Path)**:
    *   **Guard**: `InventoryReservationService.AttemptReservationAsync` locks stock.
    *   **Payment**: `StripeProcessor` creates a Payment Intent.
    *   **Result**: Returns `clientSecret` to frontend.

### Phase 4: Async Completion
12. **Trigger**: User completes 3D Secure / Payment succeeds at Stripe.
13. **Endpoint**: `POST /api/webhooks/stripe`
14. **Logic**:
    *   Verifies `Stripe-Signature`.
    *   Publishes `ExternalPaymentCaptured` event.
15. **Handler**: `PaymentCapturedHandler`
    *   Updates Order Payment status to `Captured`.
    *   If fully paid, marks Order as `Complete`.
    *   Sends Email via `OrderCompletedEmailHandler`.

---

## 4. Infrastructure Deep Dive

### Inventory Reservation (`InventoryReservationService`)
*   **Problem**: Two users buy the last item at the exact same second.
*   **Solution**: Before talking to Stripe, the system "Soft Reserves" the stock in the database transaction. If stock is 0, the checkout fails immediately ("Out of Stock"), ensuring we never take money for items we don't have.

### Payment Gateway (`StripeProcessor`)
*   **Factory Pattern**: `PaymentFactory` allows switching between Stripe, PayPal, etc.
*   **Configuration**: API Keys are stored in `PaymentMethod.PrivateMetadata` (Database), not hardcoded. This allows multi-tenant (SaaS) capabilities.

---

## 5. API Reference (Current Implementation)

| Feature | Method | Path |
| :--- | :--- | :--- |
| **Cart** | `GET` | `/api/storefront/cart` |
| | `POST` | `/api/storefront/cart/items` |
| | `POST` | `/api/storefront/cart/merge` |
| **Promotion** | `POST` | `/api/storefront/cart/promotion` |
| | `DELETE` | `/api/storefront/cart/promotion` |
| **Checkout** | `POST` | `/api/storefront/checkout/addresses` |
| | `GET` | `/api/storefront/checkout/shipping-methods` |
| | `POST` | `/api/storefront/checkout/shipping-method` |
| | `POST` | `/api/storefront/checkout/place-order` |
| **System** | `POST` | `/api/webhooks/stripe` |
| **Admin** | `PUT` | `/api/admin/settings/payment-methods/{id}/config` |

---

## 6. Conclusion
The `src` implementation is **superior** to a standard reference implementation because it handles the "Edge Cases" of real-world e-commerce:
1.  **Concurrency** (Inventory Locking).
2.  **Anonymity** (Guest -> User transition).
3.  **Resilience** (Webhook-driven completion).

It is ready for Frontend integration.
