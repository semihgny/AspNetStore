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
    public class OrderController : Controller
    {
        private readonly DataContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(DataContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> Checkout()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var cartId = HttpContext.Session.GetInt32("CartId");

                if (!cartId.HasValue)
                {
                    TempData["Error"] = "Sepetiniz boş.";
                    return RedirectToAction("Index", "Cart");
                }

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == cartId && c.IsActive);

                if (cart == null || !cart.CartItems.Any())
                {
                    TempData["Error"] = "Sepetiniz boş.";
                    return RedirectToAction("Index", "Cart");
                }

                foreach (var item in cart.CartItems)
                {
                    if (item.Quantity > item.Product.StockQuantity)
                    {
                        TempData["Error"] = $"{item.Product.Name} için yeterli stok yok.";
                        return RedirectToAction("Index", "Cart");
                    }
                }

                var user = await _context.Users.FindAsync(userId);
                var viewModel = new CheckoutViewModel
                {
                    FirstName = user?.FirstName ?? "",
                    LastName = user?.LastName ?? "",
                    Email = user?.Email ?? "",
                    Phone = user?.Phone ?? "",
                    Address = user?.Address ?? "",
                    CartItems = cart.CartItems.ToList()
                };

                return RedirectToAction("Index", "Checkout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout işlemi sırasında hata oluştu");
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
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var cartId = HttpContext.Session.GetInt32("CartId");

                if (!cartId.HasValue)
                {
                    TempData["Error"] = "Sepetiniz boş.";
                    return RedirectToAction("Index", "Cart");
                }

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == cartId && c.IsActive);

                if (cart == null || !cart.CartItems.Any())
                {
                    TempData["Error"] = "Sepetiniz boş.";
                    return RedirectToAction("Index", "Cart");
                }

                foreach (var item in cart.CartItems)
                {
                    if (item.Quantity > item.Product.StockQuantity)
                    {
                        TempData["Error"] = $"{item.Product.Name} için yeterli stok yok.";
                        return RedirectToAction("Index", "Cart");
                    }
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
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = "Pending",
                    TotalAmount = cart.TotalAmount,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    _context.OrderItems.Add(orderItem);

                    item.Product.StockQuantity -= item.Quantity;
                }

                cart.IsActive = false;
                HttpContext.Session.Remove("CartId");

                await _context.SaveChangesAsync();

                TempData["Success"] = "Siparişiniz başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş oluşturulurken hata oluştu");
                TempData["Error"] = "Sipariş oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

                if (order == null)
                {
                    TempData["Error"] = "Sipariş bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
                {
                    TempData["Error"] = "Bu sipariş artık iptal edilemez.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var item in order.OrderDetails)
                {
                    item.Product.StockQuantity += item.Quantity;
                }

                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Sipariş başarıyla iptal edildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş iptal edilirken hata oluştu");
                TempData["Error"] = "Sipariş iptal edilirken bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

                if (order == null)
                {
                    TempData["Error"] = "Sipariş bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Sipariş başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş silinirken hata oluştu");
                TempData["Error"] = "Sipariş silinirken bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(Index));
            }
        }

        private string GenerateOrderNumber()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999).ToString();
        }
    }
} 