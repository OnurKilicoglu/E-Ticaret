using Microsoft.AspNetCore.Mvc;
using ECommerce.WebUI.Models;
using ECommerce.Service.Interfaces;
using ECommerce.Core.Entities;

namespace ECommerce.WebUI.Controllers;

/// <summary>
/// Controller for user authentication and account management
/// Routes: /account/login, /account/register, /account/profile, /account/orders
/// </summary>
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly IUserService _userService;
    private readonly IAddressService _addressService;
    private readonly IOrderService _orderService;

    public AccountController(
        ILogger<AccountController> logger,
        IUserService userService,
        IAddressService addressService,
        IOrderService orderService)
    {
        _logger = logger;
        _userService = userService;
        _addressService = addressService;
        _orderService = orderService;
    }

    /// <summary>
    /// Login page
    /// GET: /account/login
    /// </summary>
    [HttpGet]
    [Route("account/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Title"] = "Login to Your Account";
        ViewData["MetaDescription"] = "Login to your account to access your orders, wishlist, and account settings.";

        return View(new LoginViewModel());
    }

    /// <summary>
    /// Process login
    /// POST: /account/login
    /// </summary>
    [HttpPost]
    [Route("account/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            try
            {
                var user = await _userService.AuthenticateAsync(model.Email, model.Password);
                
                if (user != null)
                {
                    // Simulate session/authentication (in a real app, use ASP.NET Core Identity)
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("UserName", user.UserName);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserRole", user.Role.ToString());

                    _logger.LogInformation("User {Email} logged in successfully", model.Email);

                    TempData["SuccessMessage"] = $"Welcome back, {user.FirstName ?? user.UserName}!";

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Profile");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
            }
        }

        return View(model);
    }

    /// <summary>
    /// Registration page
    /// GET: /account/register
    /// </summary>
    [HttpGet]
    [Route("account/register")]
    public IActionResult Register()
    {
        ViewData["Title"] = "Create Your Account";
        ViewData["MetaDescription"] = "Create a new account to start shopping and enjoy exclusive member benefits.";

        return View(new RegisterViewModel());
    }

    /// <summary>
    /// Process registration
    /// POST: /account/register
    /// </summary>
    [HttpPost]
    [Route("account/register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Check if email already exists
                if (!await _userService.IsEmailUniqueAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "This email address is already registered.");
                    return View(model);
                }

                // Check if username already exists
                if (!await _userService.IsUsernameUniqueAsync(model.UserName))
                {
                    ModelState.AddModelError("UserName", "This username is already taken.");
                    return View(model);
                }

                // Validate password
                var passwordValidation = _userService.ValidatePassword(model.Password);
                if (!passwordValidation.IsValid)
                {
                    foreach (var error in passwordValidation.Errors)
                    {
                        ModelState.AddModelError("Password", error);
                    }
                    return View(model);
                }

                // Create new user
                var user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Role = UserRole.Customer,
                    IsActive = true
                };

                var createdUser = await _userService.CreateUserAsync(user, model.Password);

                _logger.LogInformation("New user registered: {Email}", model.Email);

                TempData["SuccessMessage"] = "Registration successful! Please log in with your new account.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
            }
        }

        return View(model);
    }

    /// <summary>
    /// User profile page
    /// GET: /account/profile
    /// </summary>
    [HttpGet]
    [Route("account/profile")]
    public async Task<IActionResult> Profile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return RedirectToAction("Login", new { returnUrl = "/account/profile" });
        }

        try
        {
            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var viewModel = new ProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                IsActive = user.IsActive,
                LastLoginDate = user.LastLoginDate,
                LoginCount = user.LoginCount,
                CreatedDate = user.CreatedDate
            };

            ViewData["Title"] = "My Profile";
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading profile for user {UserId}", userId);
            TempData["ErrorMessage"] = "Error loading your profile. Please try again.";
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// Update user profile
    /// POST: /account/profile
    /// </summary>
    [HttpPost]
    [Route("account/profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                // Update user properties
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;

                await _userService.UpdateUserAsync(user);

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                ModelState.AddModelError("", "An error occurred while updating your profile.");
            }
        }

        return View(model);
    }

    /// <summary>
    /// User orders page
    /// GET: /account/orders
    /// </summary>
    [HttpGet]
    [Route("account/orders")]
    public async Task<IActionResult> Orders(int page = 1)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return RedirectToAction("Login", new { returnUrl = "/account/orders" });
        }

        try
        {
            // Load user orders from the database
            var (orders, totalCount) = await _orderService.GetOrdersByCustomerAsync(userId.Value, page, 10);
            
            var viewModel = new OrderHistoryViewModel
            {
                Orders = orders.Select(o => new OrderSummaryViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber ?? "",
                    OrderDate = o.OrderDate,
                    OrderStatus = o.OrderStatus.ToString(),
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems?.Count ?? 0,
                    PaymentStatus = o.Payment?.PaymentStatus.ToString() ?? "Unknown",
                    PaymentMethod = o.Payment?.PaymentMethod.ToString() ?? "Unknown",
                    ShippingAddress = o.ShippingAddress != null 
                        ? $"{o.ShippingAddress.AddressLine}, {o.ShippingAddress.City}, {o.ShippingAddress.State} {o.ShippingAddress.ZipCode}"
                        : "",
                    CanCancel = o.OrderStatus == OrderStatus.Pending || o.OrderStatus == OrderStatus.Processing,
                    TrackingNumber = o.TrackingNumber
                }).ToList(),
                CurrentPage = page,
                TotalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / 10) : 0,
                TotalItems = totalCount
            };

            ViewData["Title"] = "My Orders";
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading orders for user {UserId}", userId);
            TempData["ErrorMessage"] = "Error loading your orders. Please try again.";
            return RedirectToAction("Profile");
        }
    }

    /// <summary>
    /// Access denied page
    /// GET: /account/accessdenied
    /// </summary>
    [HttpGet]
    [Route("account/accessdenied")]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied";
        return View();
    }

    /// <summary>
    /// Logout
    /// POST: /account/logout
    /// </summary>
    [HttpPost]
    [Route("account/logout")]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["SuccessMessage"] = "You have been logged out successfully.";
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Check if current user is authenticated
    /// </summary>
    private bool IsUserAuthenticated()
    {
        return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
    }

    /// <summary>
    /// Get current user ID from session
    /// </summary>
    private int? GetCurrentUserId()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (int.TryParse(userIdString, out int userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// Wishlist page
    /// GET: /account/wishlist
    /// </summary>
    [HttpGet]
    [Route("account/wishlist")]
    public async Task<IActionResult> Wishlist()
    {
        if (!IsUserAuthenticated())
        {
            return RedirectToAction("Login", new { returnUrl = Url.Action("Wishlist") });
        }

        ViewData["Title"] = "My Wishlist";
        ViewData["MetaDescription"] = "Manage your wishlist and save your favorite products for later.";

        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            // For now, return empty wishlist. This would be populated from a wishlist service
            var viewModel = new WishlistViewModel
            {
                Items = new List<WishlistItemViewModel>(),
                TotalItems = 0
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading wishlist for user");
            TempData["ErrorMessage"] = "Unable to load wishlist. Please try again.";
            return View(new WishlistViewModel());
        }
    }

    /// <summary>
    /// Add product to wishlist
    /// POST: /account/wishlist/add
    /// </summary>
    [HttpPost]
    [Route("account/wishlist/add")]
    public async Task<IActionResult> AddToWishlist(int productId)
    {
        if (!IsUserAuthenticated())
        {
            return Json(new { success = false, message = "Please login to add items to wishlist." });
        }

        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Invalid user session." });
            }

            // TODO: Implement wishlist service
            // await _wishlistService.AddToWishlistAsync(userId.Value, productId);

            return Json(new { success = true, message = "Product added to wishlist!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to wishlist", productId);
            return Json(new { success = false, message = "Unable to add product to wishlist." });
        }
    }

    /// <summary>
    /// Remove product from wishlist
    /// POST: /account/wishlist/remove
    /// </summary>
    [HttpPost]
    [Route("account/wishlist/remove")]
    public async Task<IActionResult> RemoveFromWishlist(int productId)
    {
        if (!IsUserAuthenticated())
        {
            return Json(new { success = false, message = "Please login to manage wishlist." });
        }

        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Invalid user session." });
            }

            // TODO: Implement wishlist service
            // await _wishlistService.RemoveFromWishlistAsync(userId.Value, productId);

            return Json(new { success = true, message = "Product removed from wishlist!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing product {ProductId} from wishlist", productId);
            return Json(new { success = false, message = "Unable to remove product from wishlist." });
        }
    }

    /// <summary>
    /// Addresses management page
    /// GET: /account/addresses
    /// </summary>
    [HttpGet]
    [Route("account/addresses")]
    public async Task<IActionResult> Addresses()
    {
        if (!IsUserAuthenticated())
        {
            return RedirectToAction("Login", new { returnUrl = Url.Action("Addresses") });
        }

        ViewData["Title"] = "My Addresses";
        ViewData["MetaDescription"] = "Manage your delivery addresses for faster checkout.";

        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            // Load user addresses from the database
            var addresses = await _addressService.GetUserAddressesAsync(userId.Value);
            var defaultAddress = addresses.FirstOrDefault(a => a.IsDefault);
            
            var viewModel = new AddressesViewModel
            {
                Addresses = addresses.Select(a => new UserAddressViewModel
                {
                    Id = a.Id,
                    Title = $"{a.FirstName} {a.LastName}",
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    AddressLine1 = a.AddressLine,
                    AddressLine2 = a.AddressLine2,
                    City = a.City,
                    State = a.State,
                    PostalCode = a.ZipCode,
                    Country = a.Country,
                    Phone = a.PhoneNumber,
                    IsDefault = a.IsDefault,
                    CreatedDate = a.CreatedDate
                }).ToList(),
                NewAddress = new UserAddressViewModel(),
                DefaultAddressId = defaultAddress?.Id
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading addresses for user");
            TempData["ErrorMessage"] = "Unable to load addresses. Please try again.";
            return View(new AddressesViewModel());
        }
    }

    /// <summary>
    /// Add new address
    /// POST: /account/addresses/add
    /// </summary>
    [HttpPost]
    [Route("account/addresses/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress(UserAddressViewModel model)
    {
        if (!IsUserAuthenticated())
        {
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            var viewModel = new AddressesViewModel
            {
                Addresses = new List<UserAddressViewModel>(),
                NewAddress = model
            };
            return View("Addresses", viewModel);
        }

        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            // Create new address entity
            var address = new ECommerce.Core.Entities.ShippingAddress
            {
                AppUserId = userId.Value,
                FirstName = model.FirstName,
                LastName = model.LastName,
                AddressLine = model.AddressLine1,
                AddressLine2 = model.AddressLine2,
                City = model.City,
                State = model.State,
                ZipCode = model.PostalCode,
                Country = model.Country,
                PhoneNumber = model.Phone,
                IsDefault = model.IsDefault
            };

            await _addressService.AddAddressAsync(address);

            TempData["SuccessMessage"] = "Address added successfully!";
            return RedirectToAction("Addresses");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding address for user");
            TempData["ErrorMessage"] = "Unable to add address. Please try again.";
            return RedirectToAction("Addresses");
        }
    }

    /// <summary>
    /// Settings page
    /// GET: /account/settings
    /// </summary>
    [HttpGet]
    [Route("account/settings")]
    public async Task<IActionResult> Settings()
    {
        if (!IsUserAuthenticated())
        {
            return RedirectToAction("Login", new { returnUrl = Url.Action("Settings") });
        }

        ViewData["Title"] = "Account Settings";
        ViewData["MetaDescription"] = "Manage your account preferences and privacy settings.";

        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            // For now, return default settings. This would be populated from user service
            var viewModel = new UserSettingsViewModel
            {
                FirstName = HttpContext.Session.GetString("UserName") ?? "",
                LastName = "",
                Email = HttpContext.Session.GetString("UserEmail") ?? "",
                EmailNotifications = true,
                MarketingEmails = true,
                OrderUpdates = true,
                ProductRecommendations = true,
                PreferredLanguage = "en",
                PreferredCurrency = "USD",
                TimeZone = "UTC"
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings for user");
            TempData["ErrorMessage"] = "Unable to load settings. Please try again.";
            return View(new UserSettingsViewModel());
        }
    }

    /// <summary>
    /// Update settings
    /// POST: /account/settings
    /// </summary>
    [HttpPost]
    [Route("account/settings")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(UserSettingsViewModel model)
    {
        if (!IsUserAuthenticated())
        {
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            // TODO: Implement settings service
            // await _settingsService.UpdateSettingsAsync(userId.Value, model);

            TempData["SuccessMessage"] = "Settings updated successfully!";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings for user");
            TempData["ErrorMessage"] = "Unable to update settings. Please try again.";
            return View(model);
        }
    }
}
