/*
 * E-Commerce Site JavaScript
 * Modern, responsive e-commerce functionality
 */

$(document).ready(function() {
    // Initialize tooltips
    initializeTooltips();
    
    // Initialize back to top button
    initializeBackToTop();
    
    // Initialize cart functionality
    initializeCart();
    
    // Initialize search functionality
    initializeSearch();
    
    // Initialize wishlist functionality
    initializeWishlist();
    
    // Load cart count on page load
    updateCartCount();
    
    // Initialize toast notifications
    initializeToasts();
});

// Initialize Bootstrap tooltips
function initializeTooltips() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function(tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Back to top button functionality
function initializeBackToTop() {
    const backToTopBtn = $('#backToTop');
    
    $(window).scroll(function() {
        if ($(this).scrollTop() > 100) {
            backToTopBtn.addClass('show');
        } else {
            backToTopBtn.removeClass('show');
        }
    });
    
    backToTopBtn.on('click', function(e) {
        e.preventDefault();
        $('html, body').animate({scrollTop: 0}, 300);
    });
}

// Cart functionality
function initializeCart() {
    // Add to cart buttons
    $(document).on('click', '.btn-add-to-cart', function(e) {
        e.preventDefault();
        
        const productId = $(this).data('product-id');
        const productName = $(this).data('product-name');
        const productPrice = $(this).data('product-price');
        const quantity = 1; // Default quantity
        
        addToCart(productId, productName, productPrice, quantity);
    });
}

// Add item to cart
function addToCart(productId, productName, productPrice, quantity) {
    // Show loading state
    const btn = $(`.btn-add-to-cart[data-product-id="${productId}"]`);
    const originalText = btn.html();
    btn.prop('disabled', true).html('<span class="loading-spinner"></span> Adding...');
    
    // Make API call to add to cart
    fetch('/cart/add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        body: JSON.stringify({
            productId: productId,
            quantity: quantity
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Update cart count
            updateCartCount(data.cartCount);
            
            // Show success message
            showToast('Success', data.message, 'success');
            
            // Add visual feedback
            btn.closest('.card').addClass('added-to-cart');
            setTimeout(function() {
                btn.closest('.card').removeClass('added-to-cart');
            }, 1000);
        } else {
            showToast('Error', data.message, 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showToast('Error', 'Failed to add item to cart', 'error');
    })
    .finally(() => {
        // Reset button
        btn.prop('disabled', false).html(originalText);
    });
}

// Update cart count in header
function updateCartCount(newCount) {
    const cartCountEl = $('.cart-count');
    
    if (typeof newCount !== 'undefined') {
        cartCountEl.text(newCount);
    } else {
        // Fetch current count from server
        fetch('/cart/count')
            .then(response => response.json())
            .then(data => {
                cartCountEl.text(data.cartCount);
            })
            .catch(error => {
                console.error('Error fetching cart count:', error);
            });
    }
    
    // Animate the badge
    cartCountEl.addClass('animate-bounce');
    setTimeout(function() {
        cartCountEl.removeClass('animate-bounce');
    }, 500);
}

// Search functionality
function initializeSearch() {
    let searchTimeout;
    const searchInput = $('.search-input');
    const searchResults = $('<div class="search-results position-absolute bg-white border rounded shadow-lg w-100" style="top: 100%; z-index: 1000; display: none;"></div>');
    
    searchInput.parent().addClass('position-relative').append(searchResults);
    
    searchInput.on('input', function() {
        const query = $(this).val().trim();
        
        clearTimeout(searchTimeout);
        
        if (query.length >= 2) {
            searchTimeout = setTimeout(function() {
                performQuickSearch(query, searchResults);
            }, 300);
        } else {
            searchResults.hide();
        }
    });
    
    // Hide search results when clicking outside
    $(document).on('click', function(e) {
        if (!$(e.target).closest('.search-box').length) {
            searchResults.hide();
        }
    });
}

// Perform quick search
function performQuickSearch(query, resultsContainer) {
    $.get('/Catalog/QuickSearch', { q: query })
        .done(function(response) {
            if (response.suggestions && response.suggestions.length > 0) {
                let html = '<div class="p-2"><small class="text-muted">Quick Results:</small></div>';
                
                response.suggestions.forEach(function(item) {
                    html += `
                        <a href="${item.url}" class="search-result-item p-2 border-bottom text-decoration-none text-dark d-block">
                            <div class="d-flex align-items-center">
                                <img src="${item.image}" alt="${item.name}" class="me-2" style="width: 40px; height: 40px; object-fit: cover; border-radius: 4px;">
                                <div class="flex-grow-1">
                                    <div class="fw-medium">${item.name}</div>
                                    <div class="text-primary small">${item.price}</div>
                                </div>
                                <i class="bi bi-arrow-right text-muted"></i>
                            </div>
                        </a>
                    `;
                });
                
                html += `<div class="p-2 text-center"><a href="/search?q=${encodeURIComponent(query)}" class="btn btn-outline-primary btn-sm">View All Results</a></div>`;
                
                resultsContainer.html(html).show();
            } else {
                resultsContainer.html('<div class="p-3 text-center text-muted">No results found</div>').show();
            }
        })
        .fail(function() {
            resultsContainer.hide();
        });
}

// Toast notifications
function initializeToasts() {
    // Auto-show existing toasts
    $('.toast').each(function() {
        const toast = new bootstrap.Toast(this);
        toast.show();
    });
}

// Show custom toast notification
function showToast(title, message, type = 'info') {
    const toastContainer = $('.toast-container');
    if (toastContainer.length === 0) {
        $('body').append('<div class="toast-container position-fixed top-0 end-0 p-3"></div>');
    }
    
    const typeClasses = {
        'success': 'bg-success',
        'error': 'bg-danger',
        'warning': 'bg-warning',
        'info': 'bg-info'
    };
    
    const typeIcons = {
        'success': 'bi-check-circle',
        'error': 'bi-x-circle',
        'warning': 'bi-exclamation-triangle',
        'info': 'bi-info-circle'
    };
    
    const toastHtml = `
        <div class="toast" role="alert">
            <div class="toast-header ${typeClasses[type]} text-white">
                <i class="bi ${typeIcons[type]} me-2"></i>
                <strong class="me-auto">${title}</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;
    
    const toastElement = $(toastHtml);
    $('.toast-container').append(toastElement);
    
    const toast = new bootstrap.Toast(toastElement[0]);
    toast.show();
    
    // Remove element after hiding
    toastElement.on('hidden.bs.toast', function() {
        $(this).remove();
    });
}

// Initialize wishlist functionality
function initializeWishlist() {
    // Handle add to wishlist button clicks
    $(document).on('click', '.add-to-wishlist-btn', function(e) {
        e.preventDefault();
        e.stopPropagation();
        
        const productId = $(this).data('product-id');
        const productName = $(this).data('product-name');
        const button = $(this);
        const icon = button.find('i');
        
        // Check if user is logged in
        if (!isUserLoggedIn()) {
            showToast('Please login to add items to your wishlist', 'warning');
            return;
        }
        
        // Show loading state
        const originalIcon = icon.attr('class');
        icon.attr('class', 'bi bi-heart-fill text-danger');
        button.prop('disabled', true);
        
        // Add to wishlist
        $.post('/account/wishlist/add', { productId: productId })
            .done(function(response) {
                if (response.success) {
                    showToast(response.message || 'Product added to wishlist!', 'success');
                    icon.attr('class', 'bi bi-heart-fill text-danger');
                    button.attr('title', 'Added to Wishlist');
                } else {
                    showToast(response.message || 'Failed to add to wishlist', 'error');
                    icon.attr('class', originalIcon);
                }
            })
            .fail(function() {
                showToast('Error adding to wishlist', 'error');
                icon.attr('class', originalIcon);
            })
            .always(function() {
                button.prop('disabled', false);
            });
    });
}

// Check if user is logged in
function isUserLoggedIn() {
    // Simple check - you can enhance this based on your auth implementation
    return document.querySelector('.navbar .dropdown-item[href*="Profile"]') !== null;
}

// Utility functions
function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(amount);
}

// Add CSS animations
const additionalCSS = `
    <style>
        .loading-spinner {
            display: inline-block;
            width: 1rem;
            height: 1rem;
            border: 2px solid transparent;
            border-top: 2px solid currentColor;
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }
        
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        
        .animate-bounce {
            animation: bounce 0.5s ease-in-out;
        }
        
        @keyframes bounce {
            0%, 100% { transform: scale(1); }
            50% { transform: scale(1.2); }
        }
        
        .added-to-cart {
            animation: pulse 1s ease-in-out;
        }
        
        @keyframes pulse {
            0% { transform: scale(1); }
            50% { transform: scale(1.02); box-shadow: 0 0 20px rgba(37, 99, 235, 0.3); }
            100% { transform: scale(1); }
        }
        
        .search-result-item {
            cursor: pointer;
            transition: background-color 0.2s ease;
        }
        
        .search-result-item:hover {
            background-color: #f8f9fa;
        }
        
        .btn-back-to-top {
            transition: all 0.3s ease;
            opacity: 0;
            visibility: hidden;
        }
        
        .btn-back-to-top.show {
            opacity: 1;
            visibility: visible;
        }
    </style>
`;

$('head').append(additionalCSS);

// Initialize wishlist functionality
function initializeWishlist() {
    // Handle add to wishlist button clicks
    $(document).on('click', '.add-to-wishlist-btn', function(e) {
        e.preventDefault();
        e.stopPropagation();
        
        const productId = $(this).data('product-id');
        const productName = $(this).data('product-name');
        const button = $(this);
        const icon = button.find('i');
        
        // Check if user is logged in
        if (!isUserLoggedIn()) {
            showToast('Please login to add items to your wishlist', 'warning');
            return;
        }
        
        // Show loading state
        const originalIcon = icon.attr('class');
        icon.attr('class', 'bi bi-heart-fill text-danger');
        button.prop('disabled', true);
        
        // Add to wishlist
        $.post('/account/wishlist/add', { productId: productId })
            .done(function(response) {
                if (response.success) {
                    showToast(response.message || 'Product added to wishlist!', 'success');
                    icon.attr('class', 'bi bi-heart-fill text-danger');
                    button.attr('title', 'Added to Wishlist');
                } else {
                    showToast(response.message || 'Failed to add to wishlist', 'error');
                    icon.attr('class', originalIcon);
                }
            })
            .fail(function() {
                showToast('Error adding to wishlist', 'error');
                icon.attr('class', originalIcon);
            })
            .always(function() {
                button.prop('disabled', false);
            });
    });
}

// Check if user is logged in using server-side session data
function isUserLoggedIn() {
    // Use the global user session data provided by the server
    return window.userSessionData && window.userSessionData.isLoggedIn === 'true';
}
