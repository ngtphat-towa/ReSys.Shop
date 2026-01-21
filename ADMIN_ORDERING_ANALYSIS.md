# Admin Ordering Analysis & Fix Report

## 1. Overview
The Admin Ordering module allows back-office staff to manually create, edit, and process orders. This is critical for phone sales, B2B quotes, and resolving customer issues.

**Status:** ✅ **Fixed & Operational** (previously broken).

---

## 2. Fixed Issues (The "Why")

### Issue 1: The "Ghost Stock" Problem
**Before:** Admins could add items to an order and click "Advance" all the way to completion without ever checking if the items were in stock.
**The Fix:** Updated `AdvanceOrderState.cs`.
*   **Logic**: When transitioning from `Delivery` → `Payment`, the system now calls `InventoryReservationService`.
*   **Result**: If stock is unavailable, the Admin receives an error immediately. They cannot oversell.

### Issue 2: The "Missing Shipment" Blocker
**Before:** Manual orders skipped the automated shipment creation logic found in checkout. When Admins tried to "Complete" the order, it failed validation (`Shipments.Any()` check).
**The Fix:** Added auto-shipment generation to `AdvanceOrderState.cs`.
*   **Logic**: After reserving inventory, the system groups items by location and creates `Shipment` records.
*   **Result**: The order satisfies invariants and can be successfully completed.

---

## 3. The New Admin Workflow

1.  **Create Order**: `CreateManualOrder` (Status: `Cart`).
2.  **Add Items**: `AddVariantToManualOrder` (Supports Price Override).
3.  **Set Logistics**: `SetManualOrderLogistics` (Shipping Method).
4.  **Advance State**: `AdvanceOrderState` (Cart -> Address -> Delivery -> Payment).
    *   *Trigger*: At `Payment` transition, **Stock is Reserved** and **Shipments are Created**.
5.  **Payment**: `AddManualPayment` (Cash/Wire/Card).
6.  **Completion**: `AdvanceOrderState` (Payment -> Confirm -> Complete).

---

## 4. ERP Alignment Scorecard

| Requirement | Status | Notes |
| :--- | :---: | :--- |
| **Inventory Control** | 🟢 | Admin actions now respect physical stock limits. |
| **Audit Trail** | 🟢 | Price overrides (`OverridePriceCents`) are logged in History. |
| **Process Integrity** | 🟢 | State machine prevents "jumping" steps (e.g., shipping before address). |
| **Financials** | 🟢 | Totals match Line Items + Adjustments exactly. |

The Admin Ordering module is now safe for production use.
