# ERP Alignment & Business Correctness Analysis

## 1. Executive Scorecard

| Component | Rating | Assessment |
| :--- | :---: | :--- |
| **Financial Integrity** | 🟢 **A** | Immutable snapshots (`LineItem` prices) ensure historical accuracy. Payments are strictly tracked via `Payment` entity. |
| **Auditability** | 🟢 **A** | Every state change is logged in `OrderHistory`. Webhook signatures are verified. Metadata links Providers to Orders. |
| **Inventory Control** | 🔴 **D** | **Critical Risk**. Stock is not reserved at checkout. Money is taken before allocation is guaranteed. High risk of overselling. |
| **Separation of Concerns** | 🟡 **B** | Good Clean Architecture (Core vs Infra). However, "Allocation" logic is missing from the Checkout flow. |
| **Scalability** | 🟢 **A-** | Async Webhooks and CQRS pattern allow high throughput. Database locking on Inventory needs verification. |

---

## 2. Business Logic Deep Dive

### ✅ The Good: Strong Foundation
*   **Snapshotting**: When a user adds an item to the cart, the system copies the `Product Name` and `Price` to the `LineItem`. This is **ERP-Correct**. If you change the product price tomorrow, the order record remains historically accurate.
*   **State Machine**: The `Order.State` transitions (`Cart` -> `Address` -> `Delivery` -> `Payment` -> `Complete`) are strictly guarded. You cannot pay for an order without shipping addresses. This prevents "Bad Data" from entering the ledger.
*   **Traceability**: The `StripeProcessor` attaches `OrderId` and `PaymentId` to the Stripe Payment Intent metadata. This allows 100% reconciliation between the Bank (Stripe) and the ERP (Database).

### ❌ The Bad: Critical Business Gaps

#### 1. The "Overselling" Risk (Inventory Gap)
**Current Flow:**
1.  User adds item to cart (No stock check/reservation).
2.  User pays money (`PlaceOrder` -> Stripe).
3.  **Risk**: If 10 users buy the last item simultaneously, Stripe accepts all 10 payments.
4.  Order is "Paid".
5.  ... Wait for Allocation ...
6.  `Order.Complete()` fails for 9 users because `InventoryUnit` cannot be allocated.
7.  **Result**: You have 9 angry customers who paid for items you don't have.

**Correct ERP Flow:**
1.  **Soft Reserve**: When entering "Checkout" (or "Place Order"), temporarily decrement `Available Stock`.
2.  **Lock**: Ensure atomic database transaction.
3.  **Pay**: Only take money if Lock succeeded.
4.  **Commit/Rollback**: If payment fails, release stock.

#### 2. The Missing "Invoice" Document
An ERP typically separates the **Order** (Request for Goods) from the **Invoice** (Legal Financial Document).
*   **Current**: The `Order` acts as both.
*   **Issue**: If you edit an address *after* payment (to fix a typo), you technically altered the legal document associated with the payment.
*   **Correctness**: Acceptable for "Shop" level, but strict ERPs require an immutable `Invoice` snapshot generated at payment time.

#### 3. Refund & Cancellation Voids
*   **Current**: `Order.Cancel()` releases inventory units (`unit.Cancel()`), which is good.
*   **Gap**: It does **not** automatically trigger a refund in Stripe. The `StripeProcessor` has no connection to `Order.Cancel()`.
*   **Operational Risk**: A support agent cancels an order in the Admin panel, thinking it refunds the customer. It doesn't. You keep the money, customer gets nothing.

---

## 3. Recommendations for "True" ERP Status

### Priority 1: Fix Inventory Concurrency
*   **Action**: Inject `IInventoryService` into `PlaceOrder.Handler`.
*   **Logic**: Call `inventory.TryReserve(orderId, items)` *before* calling `paymentFactory.GetProcessor(...)`.
*   **Failure**: If reservation fails, return `Error.Conflict("OutOfStock")` and do not call Stripe.

### Priority 2: Automate Refunds
*   **Action**: Create `OrderCanceledHandler` in `Infrastructure`.
*   **Logic**: Listen for `OrderCanceled` event. If `Order.Payments` contains captured funds, call `IPaymentProcessor.RefundAsync()`.

### Priority 3: Reconciliation Job
*   **Action**: Create a Background Service (`ReconciliationWorker`).
*   **Logic**: Every night, fetch "Succeeded" payments from Stripe API and compare with "Paid" orders in DB. Alert on discrepancies.
