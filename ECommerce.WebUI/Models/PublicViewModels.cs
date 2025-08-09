/*
 * Public WebUI ViewModels for E-Commerce Site
 * 
 * This file contains all ViewModels used in the public-facing website.
 * Data is sourced from existing Admin database entities via services:
 * - Categories, Products, Sliders, BlogPosts, FAQs, FAQCategories, ContactMessages
 * 
 * All controllers use these lightweight ViewModels to avoid exposing domain entities
 * directly to the presentation layer, ensuring clean architecture separation.
 */

using System.ComponentModel.DataAnnotations;
using ECommerce.Core.Entities;

namespace ECommerce.WebUI.Models;

#region Home Page ViewModels

/// <summary>
/// Main ViewModel for the Home page containing all sections
/// </summary>
public class HomeViewModel
{
    public List<SliderViewModel> Sliders { get; set; } = new();
    public List<ProductCardViewModel> FeaturedProducts { get; set; } = new();
    public List<ProductCardViewModel> NewProducts { get; set; } = new();
    public List<BlogCardViewModel> LatestPosts { get; set; } = new();
    public List<FaqTeaserViewModel> FaqTeasers { get; set; } = new();
    public NewsletterViewModel Newsletter { get; set; } = new();
}

/// <summary>
/// ViewModel for hero slider items
/// </summary>
public class SliderViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? LinkText { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// ViewModel for newsletter subscription
/// </summary>
public class NewsletterViewModel
{
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
    
    [Display(Name = "First Name")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; set; }
    
    public bool AcceptPrivacyPolicy { get; set; }
}

#endregion

#region Product ViewModels

/// <summary>
/// Reusable ViewModel for product cards
/// </summary>
public class ProductCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsNew => CreatedDate >= DateTime.UtcNow.AddDays(-30);
    public DateTime CreatedDate { get; set; }
    
    // Computed properties
    public string FormattedPrice => Price.ToString("C");
    public string FormattedDiscountPrice => DiscountPrice?.ToString("C") ?? "";
    public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice < Price;
    public decimal DiscountPercentage => HasDiscount ? Math.Round(((Price - DiscountPrice!.Value) / Price) * 100) : 0;
    public bool InStock => StockQuantity > 0;
    public string StockStatus => StockQuantity switch
    {
        0 => "Out of Stock",
        <= 5 => "Low Stock",
        _ => "In Stock"
    };
    public string StockBadgeClass => StockQuantity switch
    {
        0 => "danger",
        <= 5 => "warning",
        _ => "success"
    };
    public string ProductUrl => $"/product/{Slug}";
}

/// <summary>
/// ViewModel for product detail page
/// </summary>
public class ProductDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? LongDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public List<ProductCardViewModel> RelatedProducts { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    
    // SEO Properties
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
    
    // Computed properties
    public string FormattedPrice => Price.ToString("C");
    public string FormattedDiscountPrice => DiscountPrice?.ToString("C") ?? "";
    public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice < Price;
    public decimal DiscountPercentage => HasDiscount ? Math.Round(((Price - DiscountPrice!.Value) / Price) * 100) : 0;
    public bool InStock => StockQuantity > 0;
    public string PrimaryImageUrl => ImageUrls.FirstOrDefault() ?? "/images/no-image.jpg";
    public bool HasGallery => ImageUrls.Count > 1;
}

#endregion

#region Catalog ViewModels

/// <summary>
/// ViewModel for catalog/category listing page
/// </summary>
public class CatalogViewModel
{
    public PublicCategoryViewModel? CurrentCategory { get; set; }
    public PagedProductListViewModel Products { get; set; } = new();
    public CatalogFiltersViewModel Filters { get; set; } = new();
    public List<PublicCategoryViewModel> Categories { get; set; } = new();
    
    // SEO
    public string PageTitle { get; set; } = "Products";
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for catalog filters
/// </summary>
public class CatalogFiltersViewModel
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortOrder { get; set; } = "asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    
    // Helper properties
    public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || 
                             CategoryId.HasValue || 
                             MinPrice.HasValue || 
                             MaxPrice.HasValue || 
                             InStock.HasValue;
}

/// <summary>
/// ViewModel for paginated product list
/// </summary>
public class PagedProductListViewModel
{
    public List<ProductCardViewModel> Products { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; } = 12;
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int StartItem => (CurrentPage - 1) * PageSize + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
}

/// <summary>
/// ViewModel for category information (public site)
/// </summary>
public class PublicCategoryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int ProductCount { get; set; }
    public List<PublicCategoryViewModel> SubCategories { get; set; } = new();
    public string CategoryUrl => $"/category/{Slug}";
}

#endregion

#region Blog ViewModels

/// <summary>
/// ViewModel for blog listing page
/// </summary>
public class BlogListViewModel
{
    public PagedBlogListViewModel Posts { get; set; } = new();
    public BlogFiltersViewModel Filters { get; set; } = new();
    public List<BlogCardViewModel> FeaturedPosts { get; set; } = new();
    public List<string> PopularTags { get; set; } = new();
    public List<BlogCardViewModel> RecentPosts { get; set; } = new();
    
    // SEO
    public string PageTitle { get; set; } = "Blog";
    public string MetaDescription { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for blog filters
/// </summary>
public class BlogFiltersViewModel
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public string? Tag { get; set; }
    public string? Author { get; set; }
    public string SortBy { get; set; } = "publishedDate";
    public string SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 9;
}

/// <summary>
/// ViewModel for paginated blog list
/// </summary>
public class PagedBlogListViewModel
{
    public List<BlogCardViewModel> Posts { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; } = 9;
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

/// <summary>
/// Reusable ViewModel for blog post cards
/// </summary>
public class BlogCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTime PublishedDate { get; set; }
    public int ViewCount { get; set; }
    public bool IsFeatured { get; set; }
    
    // Computed properties
    public string BlogUrl => $"/blog/{Slug}";
    public string FormattedPublishedDate => PublishedDate.ToString("MMM dd, yyyy");
    public string ReadingTime => $"{Math.Max(1, Excerpt.Length / 250)} min read";
}

/// <summary>
/// ViewModel for blog post detail page
/// </summary>
public class BlogDetailViewModel
{
    public ECommerce.Core.Entities.BlogPost Post { get; set; } = new();
    public List<ECommerce.Core.Entities.BlogPost> RelatedPosts { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    
    // SEO Properties
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
    
    // Computed properties
    public string FormattedPublishedDate => (Post.PublishedDate ?? Post.CreatedDate).ToLongDateString();
    public string ReadingTime => $"{Math.Max(1, (Post.Content?.Length ?? 0) / 1000)} min read";
    public string TagsString => string.Join(", ", Tags);
}

#endregion

#region FAQ ViewModels

/// <summary>
/// ViewModel for FAQ page
/// </summary>
public class FaqViewModel
{
    public List<FaqCategoryBlockViewModel> CategoryBlocks { get; set; } = new();
    public FaqSearchViewModel Search { get; set; } = new();
    public List<FaqItemViewModel> PopularFaqs { get; set; } = new();
    
    // SEO
    public string PageTitle { get; set; } = "Frequently Asked Questions";
    public string MetaDescription { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for FAQ search
/// </summary>
public class FaqSearchViewModel
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public List<FaqCategoryViewModel> Categories { get; set; } = new();
}

/// <summary>
/// ViewModel for FAQ category block (category + its FAQs)
/// </summary>
public class FaqCategoryBlockViewModel
{
    public FaqCategoryViewModel Category { get; set; } = new();
    public List<FaqItemViewModel> Faqs { get; set; } = new();
}

/// <summary>
/// ViewModel for FAQ category
/// </summary>
public class FaqCategoryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public int DisplayOrder { get; set; }
    public int FaqCount { get; set; }
}

/// <summary>
/// ViewModel for individual FAQ item
/// </summary>
public class FaqItemViewModel
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int ViewCount { get; set; }
    public bool IsPopular => ViewCount > 100;
}

/// <summary>
/// ViewModel for FAQ teaser on homepage
/// </summary>
public class FaqTeaserViewModel
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string ShortAnswer { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}

#endregion

#region Contact ViewModels

/// <summary>
/// ViewModel for contact form
/// </summary>
public class ContactFormViewModel
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
    
    [Required(ErrorMessage = "Subject is required")]
    [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
    [Display(Name = "Subject")]
    public string Subject { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Message is required")]
    [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;
    
    [Display(Name = "I agree to the privacy policy")]
    public bool AcceptPrivacyPolicy { get; set; }
}

/// <summary>
/// ViewModel for contact page
/// </summary>
public class ContactViewModel
{
    public ContactFormViewModel ContactForm { get; set; } = new();
    public ContactInfoViewModel ContactInfo { get; set; } = new();
    
    // SEO
    public string PageTitle { get; set; } = "Contact Us";
    public string MetaDescription { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for contact information display
/// </summary>
public class ContactInfoViewModel
{
    public string CompanyName { get; set; } = "E-Commerce Store";
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Website { get; set; }
    public BusinessHoursViewModel BusinessHours { get; set; } = new();
    public SocialLinksViewModel SocialLinks { get; set; } = new();
}

/// <summary>
/// ViewModel for business hours
/// </summary>
public class BusinessHoursViewModel
{
    public string Weekdays { get; set; } = "Monday - Friday: 9:00 AM - 6:00 PM";
    public string Saturday { get; set; } = "Saturday: 10:00 AM - 4:00 PM";
    public string Sunday { get; set; } = "Sunday: Closed";
}

/// <summary>
/// ViewModel for social media links
/// </summary>
public class SocialLinksViewModel
{
    public string? Facebook { get; set; }
    public string? Twitter { get; set; }
    public string? Instagram { get; set; }
    public string? LinkedIn { get; set; }
    public string? YouTube { get; set; }
}

#endregion

#region Authentication ViewModels

/// <summary>
/// ViewModel for user login
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
    
    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}

/// <summary>
/// ViewModel for user registration
/// </summary>
public class RegisterViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Please confirm your password")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
    
    [Required(ErrorMessage = "You must agree to the terms and conditions")]
    [Display(Name = "I agree to the terms and conditions")]
    public bool AcceptTerms { get; set; }
}

/// <summary>
/// ViewModel for user profile
/// </summary>
public class ProfileViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int LoginCount { get; set; }
    public DateTime CreatedDate { get; set; }
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string FormattedLastLogin => LastLoginDate?.ToString("MMM dd, yyyy") ?? "Never";
    public string MemberSince => CreatedDate.ToString("MMM yyyy");
}

/// <summary>
/// ViewModel for order history
/// </summary>
public class OrderHistoryViewModel
{
    public List<OrderSummaryViewModel> Orders { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; } = 10;
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

/// <summary>
/// ViewModel for order summary in order history
/// </summary>
public class OrderSummaryViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public bool CanCancel { get; set; }
    public string? TrackingNumber { get; set; }
    
    // Computed properties
    public string FormattedOrderDate => OrderDate.ToString("MMM dd, yyyy");
    public string FormattedTotalAmount => TotalAmount.ToString("C");
    public string StatusBadgeClass => Status.ToLower() switch
    {
        "pending" => "warning",
        "processing" => "info",
        "shipped" => "primary",
        "delivered" => "success",
        "cancelled" => "danger",
        _ => "secondary"
    };
}

#endregion

#region Navigation ViewModels

/// <summary>
/// ViewModel for category navigation in header
/// </summary>
public class CategoryNavigationViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

#endregion

#region Cart ViewModels

/// <summary>
/// ViewModel for shopping cart page
/// </summary>
public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal Subtotal => Items.Sum(i => i.Total);
    public decimal ShippingCost { get; set; } = 0; // Free shipping for now
    public decimal Tax => Subtotal * 0.08m; // 8% tax
    public decimal Total => Subtotal + ShippingCost + Tax;
    public bool IsEmpty => !Items.Any();
    public int ItemCount => Items.Sum(i => i.Quantity);
    public string FormattedSubtotal => Subtotal.ToString("C");
    public string FormattedShippingCost => ShippingCost.ToString("C");
    public string FormattedTax => Tax.ToString("C");
    public string FormattedTotal => Total.ToString("C");
    public bool QualifiesForFreeShipping => Subtotal >= 99;
    public decimal AmountForFreeShipping => Math.Max(0, 99 - Subtotal);
}

/// <summary>
/// ViewModel for individual cart item
/// </summary>
public class CartItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public bool HasDiscount { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }
    public bool InStock { get; set; }
    
    // Computed properties
    public decimal Total => Price * Quantity;
    public decimal Savings => HasDiscount ? (OriginalPrice - Price) * Quantity : 0;
    public string FormattedPrice => Price.ToString("C");
    public string FormattedOriginalPrice => OriginalPrice.ToString("C");
    public string FormattedTotal => Total.ToString("C");
    public string FormattedSavings => Savings.ToString("C");
    public string ProductUrl => $"/product/{ProductSlug}";
    public bool IsLowStock => InStock && MaxQuantity <= 5;
    public string StockStatus => MaxQuantity switch
    {
        0 => "Out of Stock",
        <= 5 => $"Only {MaxQuantity} left",
        _ => "In Stock"
    };
    public string StockBadgeClass => MaxQuantity switch
    {
        0 => "danger",
        <= 5 => "warning",
        _ => "success"
    };
}

#endregion

#region Shared ViewModels

/// <summary>
/// ViewModel for breadcrumb navigation
/// </summary>
public class BreadcrumbViewModel
{
    public List<BreadcrumbItem> Items { get; set; } = new();
}

/// <summary>
/// Individual breadcrumb item
/// </summary>
public class BreadcrumbItem
{
    public string Text { get; set; } = string.Empty;
    public string? Url { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// ViewModel for FAQ page with categories and search functionality
/// </summary>
public class FaqPageViewModel
{
    public List<ECommerce.Core.Entities.FAQ> Faqs { get; set; } = new();
    public List<ECommerce.Core.Entities.FAQCategory> Categories { get; set; } = new();
    public int? SelectedCategoryId { get; set; }
    public string? Search { get; set; }
}

/// <summary>
/// ViewModel for search functionality
/// </summary>
public class SearchViewModel
{
    public string? Query { get; set; }
    public string Category { get; set; } = "all";
    public List<SearchResultViewModel> Results { get; set; } = new();
    public int TotalResults { get; set; }
}

/// <summary>
/// ViewModel for search results
/// </summary>
public class SearchResultViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Product, Blog, FAQ
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
}

/// <summary>
/// ViewModel for navigation menu
/// </summary>
public class NavigationViewModel
{
    public List<PublicCategoryViewModel> Categories { get; set; } = new();
    public SearchViewModel Search { get; set; } = new();
    public int CartItemCount { get; set; }
    public bool IsLoggedIn { get; set; }
    public string? UserName { get; set; }
}

#endregion

#region User Profile ViewModels

/// <summary>
/// ViewModel for wishlist page
/// </summary>
public class WishlistViewModel
{
    public List<WishlistItemViewModel> Items { get; set; } = new();
    public int TotalItems { get; set; }
}

/// <summary>
/// ViewModel for wishlist item
/// </summary>
public class WishlistItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public bool IsInStock { get; set; }
    public DateTime AddedDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public double? Rating { get; set; }
    public int ReviewCount { get; set; }
    
    // Computed properties
    public bool HasDiscount => OriginalPrice.HasValue && OriginalPrice > Price;
    public decimal? DiscountPercentage => HasDiscount ? Math.Round((1 - Price / OriginalPrice!.Value) * 100, 0) : null;
    public string FormattedAddedDate => AddedDate.ToShortDateString();
}

/// <summary>
/// ViewModel for addresses management
/// </summary>
public class AddressesViewModel
{
    public List<UserAddressViewModel> Addresses { get; set; } = new();
    public UserAddressViewModel NewAddress { get; set; } = new();
    public int? DefaultAddressId { get; set; }
}

/// <summary>
/// ViewModel for user address
/// </summary>
public class UserAddressViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedDate { get; set; }
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string FormattedAddress => $"{AddressLine1}{(!string.IsNullOrEmpty(AddressLine2) ? ", " + AddressLine2 : "")}, {City}, {State} {PostalCode}, {Country}";
}

/// <summary>
/// ViewModel for user settings
/// </summary>
public class UserSettingsViewModel
{
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    
    // Preferences
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public bool MarketingEmails { get; set; } = true;
    public bool OrderUpdates { get; set; } = true;
    public bool ProductRecommendations { get; set; } = true;
    public string PreferredLanguage { get; set; } = "en";
    public string PreferredCurrency { get; set; } = "USD";
    public string TimeZone { get; set; } = "UTC";
    
    // Privacy Settings
    public bool ProfileVisibility { get; set; } = false;
    public bool ShowPurchaseHistory { get; set; } = false;
    public bool AllowDataCollection { get; set; } = true;
    
    // Account Security
    public bool TwoFactorEnabled { get; set; } = false;
    public DateTime? LastPasswordChange { get; set; }
    public List<string> ActiveSessions { get; set; } = new();
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsEmailVerified { get; set; } = true;
    public bool IsPhoneVerified { get; set; } = false;
}

/// <summary>
/// ViewModel for password change
/// </summary>
public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;
    
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

#endregion

#region Checkout ViewModels

/// <summary>
/// ViewModel for checkout page
/// </summary>
public class CheckoutViewModel
{
    public List<CartItemViewModel> CartItems { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    
    // Shipping Information
    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }
    
    [Required]
    [Display(Name = "Address Line 1")]
    public string AddressLine1 { get; set; } = string.Empty;
    
    [Display(Name = "Address Line 2")]
    public string? AddressLine2 { get; set; }
    
    [Required]
    [Display(Name = "City")]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "State/Province")]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "ZIP/Postal Code")]
    public string PostalCode { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Country")]
    public string Country { get; set; } = string.Empty;
    
    // Payment Information
    [Required]
    [Display(Name = "Card Number")]
    public string CardNumber { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Cardholder Name")]
    public string CardHolderName { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Expiry Month")]
    [Range(1, 12)]
    public int ExpiryMonth { get; set; }
    
    [Required]
    [Display(Name = "Expiry Year")]
    [Range(2024, 2034)]
    public int ExpiryYear { get; set; }
    
    [Required]
    [Display(Name = "CVV")]
    [StringLength(4, MinimumLength = 3)]
    public string CVV { get; set; } = string.Empty;
    
    // Special Instructions
    [Display(Name = "Special Instructions")]
    public string? SpecialInstructions { get; set; }
    
    public bool SaveAddress { get; set; } = false;
}

/// <summary>
/// ViewModel for thank you page after successful checkout
/// </summary>
public class ThankYouViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string EstimatedDelivery { get; set; } = string.Empty;
}

#endregion



