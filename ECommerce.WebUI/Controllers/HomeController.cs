/*
 * HomeController - Public E-Commerce Website
 * 
 * This controller serves the main homepage and privacy page for the public website.
 * All data comes from existing Admin database entities via clean service interfaces:
 * 
 * Data Sources:
 * - Sliders: Hero carousel from ISliderService.GetActiveSlidersAsync()
 * - Products: Featured/New products from IProductService.GetActiveProductsAsync()
 * - Blog Posts: Latest posts from IBlogPostService.GetBlogPostsAsync()
 * - FAQs: Popular questions from IFAQService.GetActiveFAQsAsync()
 * 
 * Architecture: Clean separation using ViewModels, no DbContext in controllers.
 */

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ECommerce.WebUI.Models;
using ECommerce.Service.Interfaces;

namespace ECommerce.WebUI.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ISliderService _sliderService;
    private readonly IProductService _productService;
    private readonly IBlogPostService _blogPostService;
    private readonly IFAQService _faqService;

    public HomeController(
        ILogger<HomeController> logger,
        ISliderService sliderService,
        IProductService productService,
        IBlogPostService blogPostService,
        IFAQService faqService)
    {
        _logger = logger;
        _sliderService = sliderService;
        _productService = productService;
        _blogPostService = blogPostService;
        _faqService = faqService;
    }

    /// <summary>
    /// Homepage with hero slider, featured products, latest blog posts, and FAQ teasers
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            ViewData["Title"] = "Welcome to Our Store";
            ViewData["MetaDescription"] = "Discover amazing products, read our latest blog posts, and find answers to frequently asked questions.";

            var viewModel = new HomeViewModel();

            // Load hero sliders (active only, ordered by DisplayOrder)
            var sliders = await _sliderService.GetActiveSlidersAsync();
            viewModel.Sliders = sliders.Select(s => new SliderViewModel
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description ?? "",
                ImageUrl = s.ImageUrl ?? "/images/slider-placeholder.jpg",
                LinkUrl = s.Link,
                LinkText = s.ButtonText,
                DisplayOrder = s.DisplayOrder
            }).OrderBy(s => s.DisplayOrder).ToList();

            // Load featured and new products
            var (allProducts, _) = await _productService.GetProductsAsync(
                searchTerm: null,
                categoryId: null,
                sortBy: "createdDate",
                sortOrder: "desc",
                page: 1,
                pageSize: 20
            );

            var productCards = allProducts.Select(p => new ProductCardViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = GenerateSlug(p.Name),
                Description = p.Description ?? "",
                ShortDescription = (p.Description?.Length > 100 
                    ? p.Description.Substring(0, 97) + "..." 
                    : p.Description) ?? "",
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                ImageUrl = p.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/images/product-placeholder.jpg",
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category?.Name ?? "Uncategorized",
                IsFeatured = p.IsFeatured,
                CreatedDate = p.CreatedDate
            }).ToList();

            // Featured products (marked as featured)
            viewModel.FeaturedProducts = productCards
                .Where(p => p.IsFeatured)
                .Take(8)
                .ToList();

            // New products (created within last 30 days, not featured)
            viewModel.NewProducts = productCards
                .Where(p => p.IsNew && !p.IsFeatured)
                .Take(8)
                .ToList();

            // Load latest blog posts
            var (blogPosts, _) = await _blogPostService.GetBlogPostsAsync(
                isPublished: true,
                sortBy: "publishedDate",
                sortOrder: "desc",
                page: 1,
                pageSize: 6
            );

            viewModel.LatestPosts = blogPosts.Select(b => new BlogCardViewModel
            {
                Id = b.Id,
                Title = b.Title,
                Slug = b.Slug ?? GenerateSlug(b.Title), // Use existing slug or generate from title
                Excerpt = (b.Summary?.Length > 150 
                    ? b.Summary.Substring(0, 147) + "..." 
                    : b.Summary) ?? (b.Content.Length > 150 
                    ? b.Content.Substring(0, 147) + "..." 
                    : b.Content),
                FeaturedImageUrl = b.ImageUrl ?? "/images/blog-placeholder.jpg", // ImageUrl is the correct property
                Author = b.Author ?? "Admin",
                Category = b.Category ?? "General",
                Tags = !string.IsNullOrEmpty(b.Tags) 
                    ? b.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList()
                    : new List<string>(),
                PublishedDate = b.PublishedDate ?? b.CreatedDate,
                ViewCount = b.ViewCount,
                IsFeatured = b.IsFeatured
            }).ToList();

            // Load popular FAQs for teaser section
            var faqs = await _faqService.GetActiveFAQsAsync(limit: 4);
            viewModel.FaqTeasers = faqs.Select(f => new FaqTeaserViewModel
            {
                Id = f.Id,
                Question = f.Question,
                ShortAnswer = f.Answer.Length > 100 
                    ? f.Answer.Substring(0, 97) + "..."
                    : f.Answer,
                CategoryName = f.Category?.Name ?? "General"
            }).ToList();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading homepage");
            return View(new HomeViewModel()); // Return empty model on error
        }
    }

    /// <summary>
    /// Newsletter subscription endpoint
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Newsletter(NewsletterViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Here you would typically save to a newsletter database
                // For now, we'll just log it
                _logger.LogInformation("Newsletter subscription: {Email}, {FirstName}", 
                    model.Email, model.FirstName);

                TempData["SuccessMessage"] = "Thank you for subscribing to our newsletter!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing newsletter subscription for {Email}", model.Email);
                TempData["ErrorMessage"] = "There was an error processing your subscription. Please try again.";
            }
        }

        TempData["ErrorMessage"] = "Please correct the errors and try again.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Privacy policy page
    /// </summary>
    [HttpGet]
    public IActionResult Privacy()
    {
        ViewData["Title"] = "Privacy Policy";
        ViewData["MetaDescription"] = "Learn about our privacy policy and how we protect your personal information.";
        
        return View();
    }

    /// <summary>
    /// Error page
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Generate URL-friendly slug from text
    /// </summary>
    private static string GenerateSlug(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text.ToLowerInvariant()
                   .Replace(" ", "-")
                   .Replace("&", "and")
                   .Replace("'", "")
                   .Replace("\"", "")
                   .Replace("/", "-")
                   .Replace("\\", "-")
                   .Replace(".", "")
                   .Replace(",", "")
                   .Replace("(", "")
                   .Replace(")", "")
                   .Replace("[", "")
                   .Replace("]", "")
                   .Replace("{", "")
                   .Replace("}", "")
                   .Replace(":", "")
                   .Replace(";", "")
                   .Replace("!", "")
                   .Replace("?", "")
                   .Replace("#", "")
                   .Replace("@", "")
                   .Replace("%", "")
                   .Replace("^", "")
                   .Replace("*", "")
                   .Replace("+", "")
                   .Replace("=", "")
                   .Replace("|", "")
                   .Replace("<", "")
                   .Replace(">", "")
                   .Replace("~", "")
                   .Replace("`", "");
    }
}
