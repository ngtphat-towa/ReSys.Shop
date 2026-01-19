using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Ordering.LineItems;
using ReSys.Core.Domain.Ordering.Payments;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Domain.Location.Addresses;
using ReSys.Core.Domain.Ordering.InventoryUnits;

namespace ReSys.Core.UnitTests.Domain.Ordering;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Domain", "Order")]
public class OrderTests
{
    private readonly Guid _storeId = Guid.NewGuid();
    private readonly Address _testAddressVal = Address.Create("Street", "City", "12345", "US").Value;

    [Fact(DisplayName = "Create: Should successfully initialize order in Cart state")]
    public void Create_Should_InitializeOrder()
    {
        // Act
        var result = Order.Create(_storeId, "USD");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.State.Should().Be(Order.OrderState.Cart);
        result.Value.StoreId.Should().Be(_storeId);
        result.Value.Number.Should().StartWith(OrderConstraints.NumberPrefix);
        result.Value.Histories.Should().HaveCount(1);
        result.Value.DomainEvents.Should().ContainSingle(e => e is OrderEvents.OrderCreated);
    }

    [Fact(DisplayName = "AddVariant: Should create line item and physical unit placeholders")]
    public void AddVariant_Should_CreateLineItemAndUnits()
    {
        // Arrange
        var order = Order.Create(_storeId, "USD").Value;
        var product = Product.Create("Test Product", "SKU", 100.00m).Value;
        var variant = product.MasterVariant!;
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = order.AddVariant(variant, 2, now);

        // Assert
        result.IsError.Should().BeFalse();
        order.LineItems.Should().HaveCount(1);
        
        var item = order.LineItems.First();
        item.Quantity.Should().Be(2);
        item.PriceCents.Should().Be(10000); // 100.00 * 100
        item.InventoryUnits.Should().HaveCount(2);
        
        order.ItemTotalCents.Should().Be(20000);
        order.TotalCents.Should().Be(20000);
    }

    [Fact(DisplayName = "Next: Should flow through entire state machine successfully")]
    public void Next_Should_FlowThroughLifecycle()
    {
        // 1. Arrange: Create order with items
        var order = Order.Create(_storeId, "USD").Value;
        var product = Product.Create("Item", "SKU", 10.00m).Value;
        order.AddVariant(product.MasterVariant!, 1, DateTimeOffset.UtcNow);
        
        var addr = UserAddress.Create(Guid.NewGuid().ToString(), _testAddressVal).Value;

        // 2. Act: Cart -> Address
        order.Next().IsError.Should().BeFalse();
        order.State.Should().Be(Order.OrderState.Address);

        // 3. Act: Address -> Delivery (Requires Addresses)
        order.SetAddresses(addr, addr);
        order.Next().IsError.Should().BeFalse();
        order.State.Should().Be(Order.OrderState.Delivery);

        // 4. Act: Delivery -> Payment (Requires Shipping Method)
        order.SetShippingMethod(Guid.NewGuid(), 500);
        order.Next().IsError.Should().BeFalse();
        order.State.Should().Be(Order.OrderState.Payment);

        // 5. Act: Payment -> Confirm
        order.Next().IsError.Should().BeFalse();
        order.State.Should().Be(Order.OrderState.Confirm);
    }

    [Fact(DisplayName = "Complete: Should fail if payments are insufficient")]
    public void Complete_ShouldFail_IfInsufficientPayment()
    {
        // Arrange: Order with 15.00 total
        var order = Order.Create(_storeId, "USD").Value;
        var product = Product.Create("Item", "SKU", 10.00m).Value;
        order.AddVariant(product.MasterVariant!, 1, DateTimeOffset.UtcNow); // 10.00
        order.SetShippingMethod(Guid.NewGuid(), 500); // 5.00
        
        // Setup State for Completion
        order.SetAddresses(UserAddress.Create("U", _testAddressVal).Value, UserAddress.Create("U", _testAddressVal).Value);
        order.Next(); // Cart -> Address
        order.Next(); // Address -> Delivery
        order.Next(); // Delivery -> Payment
        order.Next(); // Payment -> Confirm

        // Allocate inventory (Required for Complete)
        foreach(var unit in order.LineItems.First().InventoryUnits) unit.Reserve(order.Id);

        // Add partial payment (10.00 instead of 15.00)
        var payment = Payment.Create(order.Id, 1000, "USD", "CreditCard").Value;
        payment.MarkAsCaptured("TXN-1");
        order.Payments.Add(payment);

        // Act
        var result = order.Next(); // Confirm -> Complete

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Order.InsufficientPayment");
    }

    [Fact(DisplayName = "Cancel: Should release all associated inventory units")]
    public void Cancel_Should_ReleaseInventory()
    {
        // Arrange
        var order = Order.Create(_storeId, "USD").Value;
        var product = Product.Create("Item", "SKU", 10.00m).Value;
        order.AddVariant(product.MasterVariant!, 1, DateTimeOffset.UtcNow);
        var unit = order.LineItems.First().InventoryUnits.First();

        // Act
        order.Cancel("Customer changed mind");

        // Assert
        order.State.Should().Be(Order.OrderState.Canceled);
        unit.State.Should().Be(InventoryUnitState.Canceled);
        order.DomainEvents.Should().Contain(e => e is OrderEvents.OrderCanceled);
    }

    [Fact(DisplayName = "SetAddresses: Should successfully update addresses in Address state")]
    public void SetAddresses_Should_UpdateAddresses()
    {
        // Arrange
        var order = Order.Create(_storeId, "USD").Value;
        order.Next(); // Move to Address state
        
        var shipping = UserAddress.Create("User1", _testAddressVal, "Home").Value;
        var billing = UserAddress.Create("User1", _testAddressVal, "Work").Value;

        // Act
        var result = order.SetAddresses(shipping, billing);

        // Assert
        result.IsError.Should().BeFalse();
        order.ShipAddress.Should().Be(shipping);
        order.BillAddress.Should().Be(billing);
        order.ShipAddressId.Should().Be(shipping.Id);
    }

    [Fact(DisplayName = "SetShippingMethod: Should update shipping total in Delivery state")]
    public void SetShippingMethod_Should_UpdateShipmentTotal()
    {
        // Arrange
        var order = Order.Create(_storeId, "USD").Value;
        var addr = UserAddress.Create("User1", _testAddressVal).Value;
        
        var product = Product.Create("Item", "SKU", 10.00m).Value;
        order.AddVariant(product.MasterVariant!, 1, DateTimeOffset.UtcNow);

        order.Next(); // To Address
        order.SetAddresses(addr, addr);
        order.Next(); // To Delivery

        var shippingMethodId = Guid.NewGuid();

        // Act
        var result = order.SetShippingMethod(shippingMethodId, 750); // $7.50

        // Assert
        result.IsError.Should().BeFalse();
        order.ShippingMethodId.Should().Be(shippingMethodId);
        order.ShipmentTotalCents.Should().Be(750);
        order.TotalCents.Should().Be(1750); // 1000 + 750
    }

    [Fact(DisplayName = "AddVariant: Should fail if order is not in Cart state")]
    public void AddVariant_ShouldFail_IfNotInCart()
    {
        // Arrange
        var order = Order.Create(_storeId, "USD").Value;
        var product = Product.Create("Item", "SKU", 10.00m).Value;
        order.AddVariant(product.MasterVariant!, 1, DateTimeOffset.UtcNow);
        
        order.Next(); // Move to Address state

        // Act
        var result = order.AddVariant(product.MasterVariant!, 1, DateTimeOffset.UtcNow);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Order.InvalidStateTransition");
    }

    [Fact(DisplayName = "RemoveLineItem: Should remove item and update totals")]
    public void RemoveLineItem_Should_UpdateTotals()
    {
        // Arrange
        var order = Order.Create(_storeId, "USD").Value;
        var product = Product.Create("Item", "SKU", 10.00m).Value;
        order.AddVariant(product.MasterVariant!, 2, DateTimeOffset.UtcNow); // 20.00
        
        var lineItemId = order.LineItems.First().Id;

        // Act
        var result = order.RemoveLineItem(lineItemId);

        // Assert
        result.IsError.Should().BeFalse();
        order.LineItems.Should().BeEmpty();
        order.TotalCents.Should().Be(0);
    }
}