# Settings Context: Shipping Methods

## Overview
The **Settings Context** manages configuration data that drives the business logic of the shop. Unlike "Master Data" (Products), Settings change the *behavior* of the system.

**Shipping Methods** define the available logistics options for customers (e.g., "Standard Shipping - $10", "Express - $25").

## Data Flow
1.  **Admin** configures `ShippingMethod` entities via API.
2.  **Customer** reaches Checkout -> Delivery step.
3.  **System** queries active `ShippingMethod`s (`GetShippingMethods`).
4.  **Order** selects a method and snapshots the `BaseCost` into `ShipmentTotalCents`.

## Architecture (CQRS)
We implement this using Vertical Slices in `ReSys.Core.Features.Admin.Settings.ShippingMethods`.

### Commands
*   `CreateShippingMethod`: Adds a new option.
*   `UpdateShippingMethod`: Changes name/cost.
*   `DeleteShippingMethod`: Soft deletes.
*   `UpdateShippingMethodStatus`: Toggles visibility (Active/Inactive).

### Queries
*   `GetShippingMethods`: Returns list for Admin grid.
*   `GetShippingMethodById`: Returns single entity.

## Integration
The `Order` domain entity explicitly references `ShippingMethodId`. The `SetShippingMethod` logic in `Order.cs` calculates the cost based on the *current* configuration of the selected method.
