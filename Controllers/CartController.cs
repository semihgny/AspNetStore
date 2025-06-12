using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnet.Data;
using aspnet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace aspnet.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly DataContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(DataContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task<Cart> GetOrCreateCartAsync()
        {
            var cartId = HttpContext.Session.GetInt32("CartId");
            if (cartId.HasValue)
            {
                var existingCart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.Id == cartId.Value);

                if (existingCart != null && existingCart.IsActive)
                {
                    return existingCart;
                }
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var newCart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Carts.Add(newCart);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("CartId", newCart.Id);

            return newCart;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var cart = await GetOrCreateCartAsync();
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .ThenInclude(p => p.Category)
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();

                var appliedCouponCode = HttpContext.Session.GetString("AppliedCouponCode");
                if (!string.IsNullOrEmpty(appliedCouponCode))
                {
                    var coupon = await _context.Coupons
                        .FirstOrDefaultAsync(c => c.Code == appliedCouponCode && c.IsActive);

                    if (coupon != null)
                    {
                        var subtotal = cartItems.Sum(ci => ci.UnitPrice * ci.Quantity);
                        if (coupon.DiscountType == "Percentage")
                        {
                            cart.DiscountAmount = subtotal * (coupon.DiscountAmount / 100);
                        }
                        else
                        {
                            cart.DiscountAmount = coupon.DiscountAmount;
                        }
                        cart.AppliedCouponCode = appliedCouponCode;
                    }
                }

                cart.UpdateTotalAmount();
                await _context.SaveChangesAsync();

                ViewBag.Cart = cart;
                return View(cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet görüntülenirken hata oluştu");
                TempData["Error"] = "Sepetiniz görüntülenirken bir hata oluştu. Lütfen tekrar deneyin.";
                return View(new List<CartItem>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                if (quantity <= 0)
                {
                    TempData["Error"] = "Geçersiz miktar.";
                    return RedirectToAction("Index", "Home");
                }

                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    TempData["Error"] = "Ürün bulunamadı.";
                    return RedirectToAction("Index", "Home");
                }

                if (!product.IsActive)
                {
                    TempData["Error"] = "Bu ürün şu anda satışta değil.";
                    return RedirectToAction("Index", "Home");
                }

                if (product.Category == null || !product.Category.IsActive)
                {
                    TempData["Error"] = "Bu ürün şu anda satışta değil.";
                    return RedirectToAction("Index", "Home");
                }

                if (quantity > product.StockQuantity)
                {
                    TempData["Error"] = "Yeterli stok yok.";
                    return RedirectToAction("Index", "Home");
                }

                var cart = await GetOrCreateCartAsync();

                var cartItem = await _context.CartItems
                    .Include(ci => ci.Product)
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);

                if (cartItem != null)
                {
                    if (cartItem.Quantity + quantity > product.StockQuantity)
                    {
                        TempData["Error"] = "Toplam miktar stok miktarını aşıyor.";
                        return RedirectToAction("Index");
                    }

                    cartItem.Quantity += quantity;
                    cartItem.UnitPrice = product.Price;
                }
                else
                {
                    cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = product.Price,
                        DateCreated = DateTime.UtcNow
                    };
                    _context.CartItems.Add(cartItem);
                }

                cart.UpdateTotalAmount();
                await _context.SaveChangesAsync();

                TempData["Success"] = $"{product.Name} sepete eklendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ürün sepete eklenirken hata oluştu. ProductId: {productId}, Quantity: {quantity}");
                TempData["Error"] = "Ürün sepete eklenirken bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    return await RemoveFromCart(id);
                }

                var cartItem = await _context.CartItems
                    .Include(ci => ci.Product)
                    .FirstOrDefaultAsync(ci => ci.Id == id);

                if (cartItem == null)
                {
                    TempData["Error"] = "Sepet öğesi bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                if (quantity > cartItem.Product.StockQuantity)
                {
                    TempData["Error"] = "Yeterli stok yok.";
                    return RedirectToAction(nameof(Index));
                }

                cartItem.Quantity = quantity;
                cartItem.UnitPrice = cartItem.Product.Price; 

                await _context.SaveChangesAsync();
                TempData["Success"] = "Miktar güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Miktar güncellenirken hata oluştu. CartItemId: {id}, Quantity: {quantity}");
                TempData["Error"] = "Miktar güncellenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

                if (cartItem == null)
                {
                    TempData["Error"] = "Sepet öğesi bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (cartItem.Cart?.UserId != userId)
                {
                    TempData["Error"] = "Bu işlem için yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                if (cartItem.Cart != null)
                {
                    cartItem.Cart.UpdateTotalAmount();
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = $"{cartItem.Product?.Name ?? "Ürün"} sepetten kaldırıldı.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ürün sepetten kaldırılırken hata oluştu. CartItemId: {cartItemId}");
                TempData["Error"] = "Ürün sepetten kaldırılırken bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var cartId = HttpContext.Session.GetInt32("CartId");
                if (cartId != null)
                {
                    var cartItems = await _context.CartItems
                        .Where(ci => ci.CartId == cartId)
                        .ToListAsync();

                    _context.CartItems.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();

                    var cart = await _context.Carts.FindAsync(cartId.Value);
                    if (cart != null)
                    {
                        cart.IsActive = false;
                        await _context.SaveChangesAsync();
                    }

                    HttpContext.Session.Remove("CartId");
                }

                TempData["Success"] = "Sepet temizlendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet temizlenirken hata oluştu");
                TempData["Error"] = "Sepet temizlenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(couponCode))
                {
                    TempData["Error"] = "Lütfen bir kupon kodu girin.";
                    return RedirectToAction(nameof(Index));
                }

                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive);

                if (coupon == null)
                {
                    TempData["Error"] = "Geçersiz kupon kodu.";
                    return RedirectToAction(nameof(Index));
                }

                var now = DateTime.Now;
                if (coupon.StartDate.HasValue && coupon.StartDate.Value > now)
                {
                    TempData["Error"] = "Bu kupon henüz aktif değil.";
                    return RedirectToAction(nameof(Index));
                }

                if (coupon.EndDate.HasValue && coupon.EndDate.Value < now)
                {
                    TempData["Error"] = "Bu kuponun süresi dolmuş.";
                    return RedirectToAction(nameof(Index));
                }

                if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                {
                    TempData["Error"] = "Bu kupon maksimum kullanım limitine ulaşmış.";
                    return RedirectToAction(nameof(Index));
                }

                var cart = await GetOrCreateCartAsync();
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();

                var subtotal = cartItems.Sum(ci => ci.UnitPrice * ci.Quantity);

                if (subtotal < coupon.MinimumCartAmount)
                {
                    TempData["Error"] = $"Bu kuponu kullanmak için minimum sepet tutarı: {coupon.MinimumCartAmount:C}";
                    return RedirectToAction(nameof(Index));
                }

                if (coupon.DiscountType == "Percentage")
                {
                    cart.DiscountAmount = subtotal * (coupon.DiscountAmount / 100);
                }
                else
                {
                    cart.DiscountAmount = coupon.DiscountAmount;
                }

                cart.AppliedCouponCode = couponCode;
                cart.UpdateTotalAmount();
                await _context.SaveChangesAsync();

                coupon.UsedCount++;
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("AppliedCouponCode", couponCode);

                TempData["Success"] = "Kupon başarıyla uygulandı.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kupon uygulanırken hata oluştu");
                TempData["Error"] = "Kupon uygulanırken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCoupon()
        {
            try
            {
                var cart = await GetOrCreateCartAsync();
                cart.DiscountAmount = 0;
                cart.AppliedCouponCode = null;
                cart.UpdateTotalAmount();
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove("AppliedCouponCode");

                TempData["Success"] = "Kupon kaldırıldı.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kupon kaldırılırken hata oluştu");
                TempData["Error"] = "Kupon kaldırılırken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
