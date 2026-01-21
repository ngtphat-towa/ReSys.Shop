# Reference Ordering System Analysis (`temp`)

## 1. Overview
The `temp` directory contains a **Reference Implementation** for the Ordering domain. It represents a fully-fledged, production-grade E-Commerce engine designed with **Domain-Driven Design (DDD)** and **CQRS**.

**Core Philosophy:** The `Order` aggregate is the single source of truth. It orchestrates all business rules, ensuring that an order cannot exist in an invalid state (e.g., "Paid" but no "Shipping Address").

---

## 2. Architecture & Patterns

### A. The Aggregate Root (`Order.cs`)
The `Order` class is massive but cohesive. It acts as a **State Machine** and a **Transaction Boundary**.
*   **State Control**: Public properties like `State` are read-only or managed via methods like `Next()`.
*   **Invariants**: Methods like `Complete()` run complex validation (Inventory check, Payment check) before allowing the state change.
*   **Encapsulation**: Child entities (`LineItem`, `Payment`) are created via the Order's methods (`AddLineItem`, `AddPayment`), ensuring parent-child consistency.

### B. Financial Precision
*   **Currency**: Stores `Currency` code (USD, EUR).
*   **Integer Math**: While properties use `decimal` in the reference, the naming `AmountCents` implies integer-based logic to avoid rounding errors.
*   **Recalculation**: `RecalculateTotals()` is the heartbeat. It sums Items + Shipping + Adjustments - Discounts every time the order changes.

### C. Feature Slices (CQRS)
The code is organized by **Feature**, not Layer.
*   `OrderModule.Create.cs`: Handles `POST /orders`.
*   `OrderModule.Shipments.Create.cs`: Handles `POST /orders/{id}/shipments`.
This separation allows independent scaling and testing of specific order operations.

---

## 3. The Order Lifecycle (Flow)

### Phase 1: Cart & Draft
1.  **Creation**: User starts a session. `Order.Create()` makes a "Cart" status order.
2.  **Item Management**: `AddLineItem()` snapshots the price at that moment. This is crucial: if the product price changes 5 minutes later, the user's cart price does **not** change unexpectedly.

### Phase 2: Checkout Information
3.  **Addresses**: `SetShippingAddress()` is called. The state machine prevents this if the cart is empty.
4.  **Shipping**: `SetShippingMethod()` calculates freight cost. The state machine prevents this if addresses are missing.

### Phase 3: Financial Commitment
5.  **Promotion**: `ApplyPromotion()` calculates discounts. It clears previous codes to ensure only one valid code applies.
6.  **Payment**: `AddPayment()` records the intent or successful charge. The order remains in "Payment" or "Confirm" state until fully paid.

### Phase 4: Fulfillment
7.  **Completion**: `Complete()` locks the order. It validates that `InventoryUnits` are reserved.
8.  **Shipment**: Warehouse staff trigger `Shipment` creation. The Order tracks these shipments vs. the line items.

---

## 4. Key Domain Logic to Adopt

### 1. Digital vs Physical
The reference explicitly handles `IsFullyDigital`.
*   **Logic**: If all items are digital, `Next()` skips the `Address` and `Delivery` states.
*   **Benefit**: Streamlined checkout for ebook/software sales.

### 2. Inventory Coordination
*   **Pre-Allocation**: Inventory is tracked via `InventoryUnit` *before* the order is complete.
*   **Granularity**: It tracks specific units (e.g., Serial Numbers), not just counts.

### 3. Metadata Extensibility
*   **PublicMetadata**: For frontend/customer data (e.g., "Gift Message").
*   **PrivateMetadata**: For backend/system data (e.g., "Fraud Score", "Stripe Charge ID").

---

## 5. Endpoints (Reference)

| Verb | Route | Action |
| :--- | :--- | :--- |
| `POST` | `/orders` | Create new Cart |
| `POST` | `/orders/{id}/items` | Add Product |
| `PUT` | `/orders/{id}/addresses` | Set Shipping/Billing |
| `PUT` | `/orders/{id}/shipping` | Select Method |
| `POST` | `/orders/{id}/promotions` | Apply Code |
| `POST` | `/orders/{id}/payments` | Record Payment |
| `POST` | `/orders/{id}/complete` | Finalize Order |
