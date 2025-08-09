using ECommerce.Core.Entities;
using ECommerce.Data.Context;
using ECommerce.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Service.Implementations;

/// <summary>
/// Service implementation for managing user shipping addresses
/// </summary>
public class AddressService : IAddressService
{
    private readonly ECommerceDbContext _context;
    private readonly ILogger<AddressService> _logger;

    public AddressService(ECommerceDbContext context, ILogger<AddressService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all addresses for a specific user
    /// </summary>
    public async Task<IEnumerable<ShippingAddress>> GetUserAddressesAsync(int userId)
    {
        try
        {
            return await _context.ShippingAddresses
                .Where(a => a.AppUserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving addresses for user {UserId}", userId);
            return new List<ShippingAddress>();
        }
    }

    /// <summary>
    /// Get a specific address by ID
    /// </summary>
    public async Task<ShippingAddress?> GetAddressByIdAsync(int addressId, int userId)
    {
        try
        {
            return await _context.ShippingAddresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.AppUserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving address {AddressId} for user {UserId}", addressId, userId);
            return null;
        }
    }

    /// <summary>
    /// Add a new address for a user
    /// </summary>
    public async Task<ShippingAddress> AddAddressAsync(ShippingAddress address)
    {
        try
        {
            // If this is the user's first address or set as default, make it default
            var userHasAddresses = await _context.ShippingAddresses
                .AnyAsync(a => a.AppUserId == address.AppUserId);
            
            if (!userHasAddresses)
            {
                address.IsDefault = true;
            }
            else if (address.IsDefault)
            {
                // If setting this as default, remove default from other addresses
                await SetOtherAddressesAsNonDefault(address.AppUserId);
            }

            address.CreatedDate = DateTime.UtcNow;
            address.UpdatedDate = DateTime.UtcNow;

            _context.ShippingAddresses.Add(address);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New address added for user {UserId}: {AddressId}", address.AppUserId, address.Id);
            return address;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding address for user {UserId}", address.AppUserId);
            throw;
        }
    }

    /// <summary>
    /// Update an existing address
    /// </summary>
    public async Task<ShippingAddress?> UpdateAddressAsync(ShippingAddress address)
    {
        try
        {
            var existingAddress = await _context.ShippingAddresses
                .FirstOrDefaultAsync(a => a.Id == address.Id && a.AppUserId == address.AppUserId);

            if (existingAddress == null)
                return null;

            // Update properties
            existingAddress.FirstName = address.FirstName;
            existingAddress.LastName = address.LastName;
            existingAddress.AddressLine = address.AddressLine;
            existingAddress.AddressLine2 = address.AddressLine2;
            existingAddress.City = address.City;
            existingAddress.State = address.State;
            existingAddress.Country = address.Country;
            existingAddress.ZipCode = address.ZipCode;
            existingAddress.PhoneNumber = address.PhoneNumber;
            existingAddress.UpdatedDate = DateTime.UtcNow;

            // Handle default address change
            if (address.IsDefault && !existingAddress.IsDefault)
            {
                await SetOtherAddressesAsNonDefault(address.AppUserId);
                existingAddress.IsDefault = true;
            }
            else if (!address.IsDefault && existingAddress.IsDefault)
            {
                // Don't allow removing default if it's the only address
                var userAddressCount = await _context.ShippingAddresses
                    .CountAsync(a => a.AppUserId == address.AppUserId);
                
                if (userAddressCount > 1)
                {
                    existingAddress.IsDefault = false;
                    // Set another address as default
                    var nextDefault = await _context.ShippingAddresses
                        .Where(a => a.AppUserId == address.AppUserId && a.Id != address.Id)
                        .OrderByDescending(a => a.CreatedDate)
                        .FirstOrDefaultAsync();
                    
                    if (nextDefault != null)
                    {
                        nextDefault.IsDefault = true;
                    }
                }
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Address {AddressId} updated for user {UserId}", address.Id, address.AppUserId);
            return existingAddress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address {AddressId} for user {UserId}", address.Id, address.AppUserId);
            return null;
        }
    }

    /// <summary>
    /// Delete an address
    /// </summary>
    public async Task<bool> DeleteAddressAsync(int addressId, int userId)
    {
        try
        {
            var address = await _context.ShippingAddresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.AppUserId == userId);

            if (address == null)
                return false;

            // If deleting the default address, set another as default
            if (address.IsDefault)
            {
                var nextDefault = await _context.ShippingAddresses
                    .Where(a => a.AppUserId == userId && a.Id != addressId)
                    .OrderByDescending(a => a.CreatedDate)
                    .FirstOrDefaultAsync();
                
                if (nextDefault != null)
                {
                    nextDefault.IsDefault = true;
                }
            }

            _context.ShippingAddresses.Remove(address);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Address {AddressId} deleted for user {UserId}", addressId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting address {AddressId} for user {UserId}", addressId, userId);
            return false;
        }
    }

    /// <summary>
    /// Set an address as default for the user
    /// </summary>
    public async Task<bool> SetDefaultAddressAsync(int addressId, int userId)
    {
        try
        {
            var address = await _context.ShippingAddresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.AppUserId == userId);

            if (address == null)
                return false;

            // Remove default from other addresses
            await SetOtherAddressesAsNonDefault(userId);

            // Set this address as default
            address.IsDefault = true;
            address.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Address {AddressId} set as default for user {UserId}", addressId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default address {AddressId} for user {UserId}", addressId, userId);
            return false;
        }
    }

    /// <summary>
    /// Get user's default address
    /// </summary>
    public async Task<ShippingAddress?> GetDefaultAddressAsync(int userId)
    {
        try
        {
            return await _context.ShippingAddresses
                .FirstOrDefaultAsync(a => a.AppUserId == userId && a.IsDefault);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving default address for user {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Helper method to set all other addresses as non-default
    /// </summary>
    private async Task SetOtherAddressesAsNonDefault(int userId)
    {
        var otherAddresses = await _context.ShippingAddresses
            .Where(a => a.AppUserId == userId && a.IsDefault)
            .ToListAsync();

        foreach (var addr in otherAddresses)
        {
            addr.IsDefault = false;
            addr.UpdatedDate = DateTime.UtcNow;
        }
    }
}
