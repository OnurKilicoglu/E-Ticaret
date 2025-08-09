using Microsoft.AspNetCore.Mvc;
using ECommerce.WebUI.Models;
using ECommerce.Service.Interfaces;
using ECommerce.Core.Entities;

namespace ECommerce.WebUI.Controllers;

/// <summary>
/// Controller for contact page and form submission
/// Routes: /contact
/// </summary>
public class ContactController : Controller
{
    private readonly ILogger<ContactController> _logger;
    private readonly IContactMessageService _contactMessageService;

    public ContactController(
        ILogger<ContactController> logger,
        IContactMessageService contactMessageService)
    {
        _logger = logger;
        _contactMessageService = contactMessageService;
    }

    /// <summary>
    /// Contact page with form and company information
    /// GET: /contact
    /// </summary>
    [HttpGet]
    [Route("contact")]
    public IActionResult Index()
    {
        var viewModel = new ContactViewModel
        {
            ContactForm = new ContactFormViewModel(),
            ContactInfo = new ContactInfoViewModel
            {
                CompanyName = "E-Commerce Store",
                Address = "123 Business Street",
                City = "Business City, BC 12345",
                Country = "United States",
                Phone = "+1 (555) 123-4567",
                Email = "contact@ecommerce-store.com",
                Website = "https://ecommerce-store.com",
                BusinessHours = new BusinessHoursViewModel
                {
                    Weekdays = "Monday - Friday: 9:00 AM - 6:00 PM",
                    Saturday = "Saturday: 10:00 AM - 4:00 PM",
                    Sunday = "Sunday: Closed"
                },
                SocialLinks = new SocialLinksViewModel
                {
                    Facebook = "https://facebook.com/ecommerce-store",
                    Twitter = "https://twitter.com/ecommerce_store",
                    Instagram = "https://instagram.com/ecommerce_store",
                    LinkedIn = "https://linkedin.com/company/ecommerce-store"
                }
            }
        };

        // SEO
        ViewData["Title"] = "Contact Us - Get in Touch";
        ViewData["MetaDescription"] = "Contact us for any questions or support. We're here to help with your shopping experience.";

        return View(viewModel);
    }

    /// <summary>
    /// Process contact form submission
    /// POST: /contact
    /// </summary>
    [HttpPost]
    [Route("contact")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Create contact message entity
                var contactMessage = new ContactMessage
                {
                    Name = model.ContactForm.Name,
                    Email = model.ContactForm.Email,
                    PhoneNumber = model.ContactForm.PhoneNumber,
                    Subject = model.ContactForm.Subject,
                    Message = model.ContactForm.Message,
                    IsRead = false,
                    IsReplied = false,
                    CreatedDate = DateTime.UtcNow
                };

                // Save to database via service
                var savedMessage = await _contactMessageService.CreateContactMessageAsync(contactMessage);

                if (savedMessage != null)
                {
                    _logger.LogInformation("Contact message submitted by {Email}: {Subject}", 
                        model.ContactForm.Email, model.ContactForm.Subject);

                    TempData["SuccessMessage"] = "Thank you for your message! We'll get back to you within 24 hours.";
                    
                    // Clear form data after successful submission
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogError("Failed to save contact message for {Email}", model.ContactForm.Email);
                    ModelState.AddModelError("", "There was an error sending your message. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing contact form submission from {Email}", model.ContactForm.Email);
                ModelState.AddModelError("", "There was an error sending your message. Please try again.");
            }
        }

        // If we get here, there was an error - reload the page with validation errors
        model.ContactInfo = new ContactInfoViewModel
        {
            CompanyName = "E-Commerce Store",
            Address = "123 Business Street",
            City = "Business City, BC 12345",
            Country = "United States",
            Phone = "+1 (555) 123-4567",
            Email = "contact@ecommerce-store.com",
            Website = "https://ecommerce-store.com",
            BusinessHours = new BusinessHoursViewModel
            {
                Weekdays = "Monday - Friday: 9:00 AM - 6:00 PM",
                Saturday = "Saturday: 10:00 AM - 4:00 PM",
                Sunday = "Sunday: Closed"
            },
            SocialLinks = new SocialLinksViewModel
            {
                Facebook = "https://facebook.com/ecommerce-store",
                Twitter = "https://twitter.com/ecommerce_store",
                Instagram = "https://instagram.com/ecommerce_store",
                LinkedIn = "https://linkedin.com/company/ecommerce-store"
            }
        };

        ViewData["Title"] = "Contact Us - Get in Touch";
        ViewData["MetaDescription"] = "Contact us for any questions or support. We're here to help with your shopping experience.";

        return View(model);
    }

    /// <summary>
    /// AJAX endpoint for quick contact (e.g., from modal or widget)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> QuickContact([FromBody] ContactFormViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var contactMessage = new ContactMessage
                {
                    Name = model.Name,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Subject = model.Subject,
                    Message = model.Message,
                    IsRead = false,
                    IsReplied = false,
                    CreatedDate = DateTime.UtcNow
                };

                var savedMessage = await _contactMessageService.CreateContactMessageAsync(contactMessage);

                if (savedMessage != null)
                {
                    _logger.LogInformation("Quick contact message submitted by {Email}: {Subject}", 
                        model.Email, model.Subject);

                    return Json(new { 
                        success = true, 
                        message = "Thank you for your message! We'll get back to you soon." 
                    });
                }
            }

            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            return Json(new { 
                success = false, 
                message = "Please correct the errors and try again.", 
                errors 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing quick contact form");
            return Json(new { 
                success = false, 
                message = "There was an error sending your message. Please try again." 
            });
        }
    }

    /// <summary>
    /// Get contact information (AJAX endpoint)
    /// </summary>
    [HttpGet]
    public IActionResult GetContactInfo()
    {
        var contactInfo = new
        {
            companyName = "E-Commerce Store",
            address = "123 Business Street",
            city = "Business City, BC 12345",
            country = "United States",
            phone = "+1 (555) 123-4567",
            email = "contact@ecommerce-store.com",
            website = "https://ecommerce-store.com",
            businessHours = new
            {
                weekdays = "Monday - Friday: 9:00 AM - 6:00 PM",
                saturday = "Saturday: 10:00 AM - 4:00 PM",
                sunday = "Sunday: Closed"
            },
            socialLinks = new
            {
                facebook = "https://facebook.com/ecommerce-store",
                twitter = "https://twitter.com/ecommerce_store",
                instagram = "https://instagram.com/ecommerce_store",
                linkedin = "https://linkedin.com/company/ecommerce-store"
            }
        };

        return Json(contactInfo);
    }
}

