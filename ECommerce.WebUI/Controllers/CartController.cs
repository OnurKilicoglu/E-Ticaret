using Microsoft.AspNetCore.Mvc;
using ECommerce.WebUI.Models;
using ECommerce.Service.Interfaces;
using ECommerce.Core.Entities;

namespace ECommerce.WebUI.Controllers;

/// <summary>
/// Controller for shopping cart functionality
/// Routes: /cart, /cart/add, /cart/update, /cart/remove, /cart/checkout
/// </summary>
public class CartController : Controller
{
    private readonly ILogger<CartController> _logger;
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;

    public CartController(
        ILogger<CartController> logger,
        IProductService productService,
        IOrderService orderService)
    {
        _logger = logger;
        _productService = productService;
        _orderService = orderService;
    }

    /// <summary>
    /// Shopping cart page
    /// GET: /cart
    /// </summary>
    [HttpGet]
    [Route("cart")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var cartItems = GetCartItemsFromSession();
            var viewModel = new CartViewModel();

            if (cartItems.Any())
            {
                // Load product details for cart items
                var productIds = cartItems.Select(c => c.ProductId).ToList();
                
                foreach (var productId in productIds)
                {
                    var product = await _productService.GetProductByIdAsync(productId);
                    if (product != null)
                    {
                        var cartItem = cartItems.First(c => c.ProductId == productId);
                        viewModel.Items.Add(new CartItemViewModel
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            ImageUrl = product.ImageUrl ?? "/images/product-placeholder.jpg",
                            Price = product.Price,
                            OriginalPrice = product.Price,
                            Quantity = cartItem.Quantity,
                            MaxQuantity = product.StockQuantity,
                            InStock = product.IsActive && product.StockQuantity > 0,
                            HasDiscount = false,
                            ProductSlug = product.Id.ToString()
                        });
                    }
                }

                // Properties are computed automatically in the ViewModel
            }

            ViewData["Title"] = "Shopping Cart";
            ViewData["MetaDescription"] = "Review your selected items and proceed to checkout.";

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cart");
            return View(new CartViewModel());
        }
    }

    /// <summary>
    /// Add product to cart
    /// POST: /cart/add
    /// </summary>
    [HttpPost]
    [Route("cart/add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        try
        {
            // Validate product exists
            var product = await _productService.GetProductByIdAsync(request.ProductId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            if (!product.IsActive || product.StockQuantity < request.Quantity)
            {
                return Json(new { success = false, message = "Product not available" });
            }

            // Get current cart items
            var cartItems = GetCartItemsFromSession();
            
            // Check if product already in cart
            var existingItem = cartItems.FirstOrDefault(c => c.ProductId == request.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                cartItems.Add(new SessionCartItem
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                });
            }

            // Save back to session
            SaveCartItemsToSession(cartItems);
            
            var totalItems = cartItems.Sum(c => c.Quantity);

            return Json(new { 
                success = true, 
                message = "Product added to cart", 
                cartCount = totalItems,
                productName = product.Name
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product to cart");
            return Json(new { success = false, message = "Error adding to cart" });
        }
    }

    /// <summary>
    /// Update cart item quantity
    /// POST: /cart/update
    /// </summary>
    [HttpPost]
    [Route("cart/update")]
    public async Task<IActionResult> UpdateCart([FromBody] UpdateCartRequest request)
    {
        try
        {
            var cartItems = GetCartItemsFromSession();
            var item = cartItems.FirstOrDefault(c => c.ProductId == request.ProductId);
            
            if (item != null)
            {
                if (request.Quantity > 0)
                {
                    item.Quantity = request.Quantity;
                }
                else
                {
                    cartItems.Remove(item);
                }
                
                SaveCartItemsToSession(cartItems);
                
                // Recalculate totals
                var subTotal = 0m;
                foreach (var cartItem in cartItems)
                {
                    var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
                    if (product != null)
                    {
                        subTotal += product.Price * cartItem.Quantity;
                    }
                }
                
                var shippingCost = subTotal >= 50 ? 0 : 9.99m;
                var tax = subTotal * 0.08m;
                var total = subTotal + shippingCost + tax;
                
                return Json(new { 
                    success = true, 
                    cartCount = cartItems.Sum(c => c.Quantity),
                    subTotal = subTotal.ToString("C"),
                    shippingCost = shippingCost.ToString("C"),
                    tax = tax.ToString("C"),
                    total = total.ToString("C")
                });
            }
            
            return Json(new { success = false, message = "Item not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart");
            return Json(new { success = false, message = "Error updating cart" });
        }
    }

    /// <summary>
    /// Remove item from cart
    /// POST: /cart/remove
    /// </summary>
    [HttpPost]
    [Route("cart/remove")]
    public IActionResult RemoveFromCart([FromBody] RemoveFromCartRequest request)
    {
        try
        {
            var cartItems = GetCartItemsFromSession();
            var item = cartItems.FirstOrDefault(c => c.ProductId == request.ProductId);
            
            if (item != null)
            {
                cartItems.Remove(item);
                SaveCartItemsToSession(cartItems);
                
                return Json(new { 
                    success = true, 
                    cartCount = cartItems.Sum(c => c.Quantity)
                });
            }
            
            return Json(new { success = false, message = "Item not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from cart");
            return Json(new { success = false, message = "Error removing from cart" });
        }
    }

    /// <summary>
    /// Get cart count for header display
    /// GET: /cart/count
    /// </summary>
    [HttpGet]
    [Route("cart/count")]
    public IActionResult GetCartCount()
    {
        var cartItems = GetCartItemsFromSession();
        return Json(new { count = cartItems.Sum(c => c.Quantity) });
    }

    /// <summary>
    /// Checkout page
    /// GET: /cart/checkout
    /// </summary>
    [HttpGet]
    [Route("cart/checkout")]
    public async Task<IActionResult> Checkout()
    {
        var cartItems = GetCartItemsFromSession();
        
        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "Your cart is empty. Please add items before proceeding to checkout.";
            return RedirectToAction("Index");
        }

        // Check if user is logged in
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            // Redirect to login with return URL
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Cart") });
        }

        ViewData["Title"] = "Checkout";
        ViewData["MetaDescription"] = "Complete your purchase and provide shipping information.";

        try
        {
            // Create checkout view model
            var viewModel = new CheckoutViewModel();
            
            // Load cart items
            foreach (var cartItem in cartItems)
            {
                var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
                if (product != null)
                {
                    viewModel.CartItems.Add(new CartItemViewModel
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ImageUrl = product.ImageUrl ?? "/images/product-placeholder.jpg",
                        Price = product.Price,
                        OriginalPrice = product.Price,
                        Quantity = cartItem.Quantity,
                        MaxQuantity = product.StockQuantity,
                        InStock = product.IsActive && product.StockQuantity > 0,
                        HasDiscount = false,
                        ProductSlug = product.Id.ToString()
                    });
                }
            }

            // Calculate totals
            viewModel.SubTotal = viewModel.CartItems.Sum(i => i.Total);
            viewModel.ShippingCost = viewModel.SubTotal >= 50 ? 0 : 9.99m; // Free shipping over $50
            viewModel.Tax = viewModel.SubTotal * 0.08m; // 8% tax
            viewModel.Total = viewModel.SubTotal + viewModel.ShippingCost + viewModel.Tax;

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading checkout page");
            TempData["ErrorMessage"] = "Unable to load checkout page. Please try again.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Process checkout
    /// POST: /cart/checkout
    /// </summary>
    [HttpPost]
    [Route("cart/checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var cartItems = GetCartItemsFromSession();
        
        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction("Index");
        }

        // Check if user is logged in
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            // Reload cart items for display
            await ReloadCartItemsForCheckout(model);
            return View(model);
        }

        try
        {
            // Create order (this would typically use an OrderService)
            var orderId = await CreateOrderAsync(model, int.Parse(userId));
            
            if (orderId > 0)
            {
                // Clear cart
                HttpContext.Session.Remove("CartItems");
                
                // Redirect to thank you page
                return RedirectToAction("ThankYou", new { orderId = orderId });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to process your order. Please try again.";
                await ReloadCartItemsForCheckout(model);
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing checkout");
            TempData["ErrorMessage"] = "An error occurred while processing your order. Please try again.";
            await ReloadCartItemsForCheckout(model);
            return View(model);
        }
    }

    /// <summary>
    /// Thank you page after successful checkout
    /// GET: /cart/thankyou
    /// </summary>
    [HttpGet]
    [Route("cart/thankyou")]
    public IActionResult ThankYou(int? orderId)
    {
        ViewData["Title"] = "Thank You!";
        ViewData["MetaDescription"] = "Thank you for your order. Your purchase has been completed successfully.";

        var viewModel = new ThankYouViewModel
        {
            OrderId = orderId ?? 0,
            OrderNumber = $"#{orderId ?? 0:D6}",
            EstimatedDelivery = DateTime.Now.AddDays(3).ToString("MMMM dd, yyyy")
        };

        return View(viewModel);
    }

    #region Helper Methods

    /// <summary>
    /// Get cart items from session
    /// </summary>
    private List<SessionCartItem> GetCartItemsFromSession()
    {
        var cartItemsJson = HttpContext.Session.GetString("CartItems");
        if (!string.IsNullOrEmpty(cartItemsJson))
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<SessionCartItem>>(cartItemsJson) ?? new List<SessionCartItem>();
        }
        return new List<SessionCartItem>();
    }

    /// <summary>
    /// Save cart items to session
    /// </summary>
    private void SaveCartItemsToSession(List<SessionCartItem> cartItems)
    {
        var cartItemsJson = System.Text.Json.JsonSerializer.Serialize(cartItems);
        HttpContext.Session.SetString("CartItems", cartItemsJson);
    }

    /// <summary>
    /// Helper method to reload cart items for checkout view
    /// </summary>
    private async Task ReloadCartItemsForCheckout(CheckoutViewModel model)
    {
        var cartItems = GetCartItemsFromSession();
        model.CartItems.Clear();

        foreach (var cartItem in cartItems)
        {
            var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
            if (product != null)
            {
                model.CartItems.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl ?? "/images/product-placeholder.jpg",
                    Price = product.Price,
                    OriginalPrice = product.Price,
                    Quantity = cartItem.Quantity,
                    MaxQuantity = product.StockQuantity,
                    InStock = product.IsActive && product.StockQuantity > 0,
                    HasDiscount = false,
                    ProductSlug = product.Id.ToString()
                });
            }
        }

        // Recalculate totals
        model.SubTotal = model.CartItems.Sum(i => i.Total);
        model.ShippingCost = model.SubTotal >= 50 ? 0 : 9.99m;
        model.Tax = model.SubTotal * 0.08m;
        model.Total = model.SubTotal + model.ShippingCost + model.Tax;
    }

    /// <summary>
    /// Helper method to create order using the order service
    /// </summary>
    private async Task<int> CreateOrderAsync(CheckoutViewModel model, int userId)
    {
        try
        {
            var cartItems = GetCartItemsFromSession();
            if (!cartItems.Any())
            {
                throw new InvalidOperationException("Cart is empty");
            }

            // Convert session cart items to CartItem entities
            var cartItemEntities = new List<ECommerce.Core.Entities.CartItem>();
            foreach (var sessionItem in cartItems)
            {
                var product = await _productService.GetProductByIdAsync(sessionItem.ProductId);
                if (product != null)
                {
                    cartItemEntities.Add(new ECommerce.Core.Entities.CartItem
                    {
                        ProductId = sessionItem.ProductId,
                        Quantity = sessionItem.Quantity,
                        Product = product
                    });
                }
            }

            // Create shipping address entity
            var shippingAddress = new ECommerce.Core.Entities.ShippingAddress
            {
                AppUserId = userId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                AddressLine = model.AddressLine1,
                AddressLine2 = model.AddressLine2,
                City = model.City,
                State = model.State,
                Country = model.Country,
                ZipCode = model.PostalCode,
                PhoneNumber = model.Phone,
                IsDefault = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            // Determine payment method from card details
            var paymentMethod = DeterminePaymentMethod(model.CardNumber);

            // Create the order using the service
            var order = await _orderService.CreateOrderAsync(
                userId, 
                cartItemEntities, 
                shippingAddress, 
                paymentMethod, 
                model.SpecialInstructions
            );

            return order.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Helper method to determine payment method from card number
    /// </summary>
    private string DeterminePaymentMethod(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber))
            return "Unknown";

        // Remove spaces and non-digits
        var digits = new string(cardNumber.Where(char.IsDigit).ToArray());
        
        if (digits.StartsWith("4"))
            return "Visa";
        else if (digits.StartsWith("5") || digits.StartsWith("2"))
            return "Mastercard";
        else if (digits.StartsWith("3"))
            return "American Express";
        else
            return "Credit Card";
    }

    #endregion
}

/// <summary>
/// Session cart item for storing in session
/// </summary>
public class SessionCartItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Request model for adding items to cart
/// </summary>
public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// Request model for updating cart items
/// </summary>
public class UpdateCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Request model for removing items from cart
/// </summary>
public class RemoveFromCartRequest
{
    public int ProductId { get; set; }
}