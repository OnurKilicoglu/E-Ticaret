using ECommerce.Core.Entities;

namespace ECommerce.Service.Interfaces;

/// <summary>
/// Service interface for managing user shipping addresses
/// </summary>
public interface IAddressService
{
    /// <summary>
    /// Get all addresses for a specific user
    /// </summary>
    Task<IEnumerable<ShippingAddress>> GetUserAddressesAsync(int userId);
    
    /// <summary>
    /// Get a specific address by ID
    /// </summary>
    Task<ShippingAddress?> GetAddressByIdAsync(int addressId, int userId);
    
    /// <summary>
    /// Add a new address for a user
    /// </summary>
    Task<ShippingAddress> AddAddressAsync(ShippingAddress address);
    
    /// <summary>
    /// Update an existing address
    /// </summary>
    Task<ShippingAddress?> UpdateAddressAsync(ShippingAddress address);
    
    /// <summary>
    /// Delete an address
    /// </summary>
    Task<bool> DeleteAddressAsync(int addressId, int userId);
    
    /// <summary>
    /// Set an address as default for the user
    /// </summary>
    Task<bool> SetDefaultAddressAsync(int addressId, int userId);
    
    /// <summary>
    /// Get user's default address
    /// </summary>
    Task<ShippingAddress?> GetDefaultAddressAsync(int userId);
}
