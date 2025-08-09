namespace ECommerce.Core.Entities;

/// <summary>
/// User role enumeration
/// </summary>
public enum UserRole
{
    Customer = 1,
    Admin = 2
}

/// <summary>
/// Order status enumeration
/// </summary>
public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Returned = 6
}

/// <summary>
/// Payment method enumeration
/// </summary>
public enum PaymentMethod
{
    CreditCard = 1,
    PayPal = 2,
    BankTransfer = 3,
    CashOnDelivery = 4
}

/// <summary>
/// Payment status enumeration
/// </summary>
public enum PaymentStatus
{
    None = 0,
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}
