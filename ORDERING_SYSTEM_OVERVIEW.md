# Ordering & Payment System Overview

## 1. Executive Summary
The **ReSys.Shop Ordering System** is a robust, modular implementation designed for high-integrity e-commerce transactions. It supports the full lifecycle from "Add to Cart" to "Order Fulfillment," integrated with Stripe for payments and an event-driven notification system.

**Key Characteristics:**
*   **Domain-Driven**: All business logic resides in the `Order` aggregate root.
*   **Event-Driven**: Asynchronous updates via Webhooks and Domain Events.
*   **Audit-Ready**: Every state change, price override, and transaction is logged.
*   **Secure**: API keys are encrypted per-tenant; Webhooks are signature-verified.

---

## 2. Architecture & Data Flow

The system follows a **CQRS** (Command Query Responsibility Segregation) pattern, orchestrated by **MediatR**.

### The Flow: User Journey
1.  **Cart**: User adds items (`AddToCart`). System creates `Order` in `Cart` state.
2.  **Checkout**: 
    *   User sets addresses (`SetCheckoutAddresses`). State -> `Address`.
    *   User selects shipping (`SetShippingMethod`). State -> `Delivery`.
3.  **Payment**:
    *   User clicks "Place Order" (`PlaceOrder`). State -> `Payment`.
    *   System creates `PaymentIntent` via `StripeProcessor`.
    *   System returns `ClientSecret` to Frontend.
4.  **Confirmation**:
    *   Stripe processes payment.
    *   Stripe sends Webhook (`payment_intent.succeeded`).
    *   `WebhookController` receives and verifies signature.
    *   `StripeProcessor` publishes `ExternalPaymentCaptured`.
    *   `PaymentCapturedHandler` updates Order to `Paid`.
    *   `Order.Complete()` is called (if fully paid).
    *   `OrderCompleted` event fires -> Sends Email.

---

## 3. Domain Deep Dive (The Core)

### A. The Order Aggregate (`Order.cs`)
The `Order` entity is the source of truth. It manages:
*   **State Machine**: Strictly enforced transitions (`Cart` -> `Address` -> `Delivery` -> `Payment` -> `Complete`).
*   **Financials**: `TotalCents`, `ItemTotalCents`, `ShipmentTotalCents` are always recalculated on change.
*   **Inventory**: `LineItem` contains `InventoryUnit`s to track physical stock allocation.

### B. Payment Entities (`Payment.cs`, `PaymentMethod.cs`)
*   **PaymentMethod**: Configuration entity. Stores provider secrets (e.g., Stripe Keys) in `PrivateMetadata`.
*   **Payment**: Transaction record. Tracks `Amount`, `Status`, and `ReferenceTransactionId` (Stripe ID).

---

## 4. Feature Reference (Implemented APIs)

### Storefront (Customer)
| Endpoint | Method | Feature | Description |
| :--- | :--- | :--- | :--- |
| `/api/storefront/cart` | GET | `GetCart` | Returns current cart items and totals. |
| `/api/storefront/cart/items` | POST | `AddToCart` | Adds variant to cart or creates new cart. |
| `/api/storefront/checkout/addresses` | POST | `SetCheckoutAddresses` | Sets Shipping/Billing addresses. |
| `/api/storefront/checkout/shipping-methods` | GET | `GetShippingMethods` | Lists valid shipping options. |
| `/api/storefront/checkout/shipping-method` | POST | `SetShippingMethod` | Selects shipping and updates order total. |
| `/api/storefront/checkout/place-order` | POST | `PlaceOrder` | Finalizes checkout, inits Stripe, returns Secret. |

### Infrastructure (System)
| Endpoint | Method | Component | Description |
| :--- | :--- | :--- | :--- |
| `/api/webhooks/stripe` | POST | `WebhookController` | Handles async payment confirmation from Stripe. |

---

## 5. Infrastructure Details

### Payment Factory & Processor
*   **`PaymentFactory`**: Resolves the correct processor based on `PaymentMethod.Type`.
*   **`StripeProcessor`**:
    *   Reads `SecretKey` dynamically from the DB entity.
    *   Creates `PaymentIntent` with `OrderReference` metadata.
    *   Verifies Webhook Signatures using `EventUtility.ConstructEvent`.

### Notifications
*   **`OrderCompletedEmailHandler`**: Listens for `OrderCompleted` event.
*   **Templates**: Uses `SystemOrderConfirmation` use-case.
*   **Context**: Populates dynamic data (`OrderId`, `Total`) into the email.

---

## 6. Next Steps for Developers
1.  **Frontend Integration**: Connect the Vue.js `ReSys.Shop` app to the `/api/storefront` endpoints.
2.  **Stripe Elements**: Use the returned `ClientSecret` in the frontend to render the Stripe Payment Element.
3.  **Admin UI**: Build the Admin page to manage `PaymentMethod` configurations (saving secrets to `PrivateMetadata`).
