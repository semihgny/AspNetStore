using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnet.Data;
using aspnet.Models;
using aspnet.Models.ViewModels;
using aspnet.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace aspnet.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly DataContext _context;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(DataContext context, ILogger<CheckoutController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var cartId = HttpContext.Session.GetInt32("CartId");
                if (cartId == null)
                {
                    TempData["Error"] = "Sepetiniz boş.";
                    return RedirectToAction("Index", "Cart");
                }

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == cartId && c.IsActive);

                if (cart == null)
                {
                    TempData["Error"] = "Sepetiniz boş.";
                    return RedirectToAction("Index", "Cart");
                }

                var cartItems = cart.CartItems.ToList();

                if (!cartItems.Any())
                {
                    TempData["Error"] = "Sepetiniz boş.";
                    return RedirectToAction("Index", "Cart");
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bilgileri bulunamadı.";
                    return RedirectToAction("Index", "Cart");
                }

                var checkoutViewModel = new CheckoutViewModel
                {
                    CartItems = cartItems,
                    Cart = cart,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone ?? "",
                    Address = user.Address ?? "",
                    City = user.City ?? "",
                    District = user.District ?? "",
                    PostalCode = user.PostalCode ?? "",
                    PaymentMethod = "CashOnDelivery"
                };

                return View(checkoutViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout sayfası yüklenirken hata oluştu");
                TempData["Error"] = "Bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogError($"Model validation failed: {errors}");
                    return View("Index", model);
                }

                var cartId = HttpContext.Session.GetInt32("CartId");
                if (cartId == null)
                {
                    TempData["Error"] = "Sepetiniz boş.";
                    return RedirectToAction("Index", "Cart");
                }

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == cartId && c.IsActive);

                if (cart == null)
                {
                    TempData["Error"] = "Sepetiniz boş.";
                    return RedirectToAction("Index", "Cart");
                }

                var cartItems = cart.CartItems.ToList();

                if (!cartItems.Any())
                {
                    TempData["Error"] = "Sepetiniz boş.";
                    return RedirectToAction("Index", "Cart");
                }

                foreach (var item in cartItems)
                {
                    if (item.Quantity > item.Product?.StockQuantity)
                    {
                        TempData["Error"] = $"{item.Product?.Name} için yeterli stok yok.";
                        return RedirectToAction("Index", "Cart");
                    }
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bilgileri bulunamadı.";
                    return RedirectToAction("Index", "Cart");
                }

                var order = new Order
                {
                    UserId = userId,
                    OrderNumber = GenerateOrderNumber(),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    City = model.City,
                    District = model.District,
                    PostalCode = model.PostalCode,
                    TotalAmount = cartItems.Sum(ci => ci.Quantity * (ci.Product?.Price ?? 0)) - cart.DiscountAmount,
                    Status = OrderStatus.Pending,
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    User = user
                };

                _context.Orders.Add(order);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Sipariş kaydedilirken hata oluştu");
                    TempData["Error"] = "Sipariş oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.";
                    return RedirectToAction("Index");
                }

                foreach (var item in cartItems)
                {
                    if (item.Product == null) continue;

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price
                    };
                    _context.OrderItems.Add(orderItem);

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price
                    };
                    _context.OrderDetails.Add(orderDetail);

                    if (item.Product != null)
                    {
                        item.Product.StockQuantity -= item.Quantity;
                        _context.Update(item.Product);
                    }
                }

                try
                {
                    cart.IsActive = false;
                    _context.Update(cart);
                    HttpContext.Session.Remove("CartId");

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Sipariş detayları kaydedilirken hata oluştu");
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    
                    TempData["Error"] = "Sipariş oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.";
                    return RedirectToAction("Index");
                }

                TempData["Success"] = "Siparişiniz başarıyla oluşturuldu.";
                return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş oluşturulurken beklenmeyen bir hata oluştu");
                TempData["Error"] = "Sipariş oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        private string GenerateOrderNumber()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999).ToString();
        }
    }
} 