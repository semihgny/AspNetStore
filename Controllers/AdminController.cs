using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnet.Data;
using aspnet.Models;
using aspnet.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace aspnet.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DataContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(DataContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["Error"] = "Sipariş bulunamadı.";
                return RedirectToAction(nameof(Orders));
            }

            return View(order);
        }

        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> SupportTickets()
        {
            var tickets = await _context.SupportTickets
                .Include(st => st.Messages)
                .Include(st => st.User)
                .OrderByDescending(st => st.CreatedAt)
                .ToListAsync();

            return View(tickets);
        }

        public async Task<IActionResult> SupportTicketDetails(int id)
        {
            var ticket = await _context.SupportTickets
                .Include(st => st.Messages)
                .Include(st => st.User)
                .FirstOrDefaultAsync(st => st.Id == id);

            if (ticket == null)
            {
                TempData["Error"] = "Destek talebi bulunamadı.";
                return RedirectToAction(nameof(SupportTickets));
            }

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    TempData["Error"] = "Sipariş bulunamadı.";
                    return RedirectToAction(nameof(Orders));
                }

                order.Status = Enum.Parse<OrderStatus>(status);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Sipariş durumu başarıyla güncellendi.";
                return RedirectToAction(nameof(Orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş durumu güncellenirken hata oluştu");
                TempData["Error"] = "Sipariş durumu güncellenirken bir hata oluştu.";
                return RedirectToAction(nameof(Orders));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserStatus(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction(nameof(Users));
                }

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Kullanıcı durumu başarıyla güncellendi.";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı durumu güncellenirken hata oluştu");
                TempData["Error"] = "Kullanıcı durumu güncellenirken bir hata oluştu.";
                return RedirectToAction(nameof(Users));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction(nameof(Users));
                }

                user.IsAdmin = !user.IsAdmin;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Kullanıcı rolü başarıyla güncellendi.";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı rolü güncellenirken hata oluştu");
                TempData["Error"] = "Kullanıcı rolü güncellenirken bir hata oluştu.";
                return RedirectToAction(nameof(Users));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Ürün bulunamadı.";
                    return RedirectToAction(nameof(Products));
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Ürün başarıyla silindi.";
                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün silinirken hata oluştu");
                TempData["Error"] = "Ürün silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Products));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    TempData["Error"] = "Kategori bulunamadı.";
                    return RedirectToAction(nameof(Categories));
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Kategori başarıyla silindi.";
                return RedirectToAction(nameof(Categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori silinirken hata oluştu");
                TempData["Error"] = "Kategori silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Categories));
            }
        }

        public IActionResult CreateProduct()
        {
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            return View(new Product { Name = "", Description = "", ImageUrl = "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, IFormFile? imageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
                        Directory.CreateDirectory(uploadsFolder);  

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        product.ImageUrl = "/uploads/products/" + uniqueFileName;
                    }

                    product.CreatedAt = DateTime.UtcNow;
                    product.IsActive = true;
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Ürün başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Products));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün oluşturulurken hata oluştu");
                ModelState.AddModelError("", "Ürün oluşturulurken bir hata oluştu.");
            }

            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            return View(product);
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Ürün bulunamadı.";
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingProduct = await _context.Products.FindAsync(product.Id);
                    if (existingProduct == null)
                    {
                        TempData["Error"] = "Ürün bulunamadı.";
                        return RedirectToAction(nameof(Products));
                    }

                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.ImageUrl = product.ImageUrl;
                    existingProduct.IsActive = product.IsActive;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Ürün başarıyla güncellendi.";
                    return RedirectToAction(nameof(Products));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün güncellenirken hata oluştu");
                ModelState.AddModelError("", "Ürün güncellenirken bir hata oluştu.");
            }

            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            return View(product);
        }

        public IActionResult CreateCategory()
        {
            return View(new Category { Name = "", Description = "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    category.CreatedAt = DateTime.UtcNow;
                    category.IsActive = true;
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Kategori başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Categories));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori oluşturulurken hata oluştu");
                ModelState.AddModelError("", "Kategori oluşturulurken bir hata oluştu.");
            }

            return View(category);
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["Error"] = "Kategori bulunamadı.";
                return RedirectToAction(nameof(Categories));
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingCategory = await _context.Categories.FindAsync(category.Id);
                    if (existingCategory == null)
                    {
                        TempData["Error"] = "Kategori bulunamadı.";
                        return RedirectToAction(nameof(Categories));
                    }

                    existingCategory.Name = category.Name;
                    existingCategory.Description = category.Description;
                    existingCategory.IsActive = category.IsActive;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Kategori başarıyla güncellendi.";
                    return RedirectToAction(nameof(Categories));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori güncellenirken hata oluştu");
                ModelState.AddModelError("", "Kategori güncellenirken bir hata oluştu.");
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupportTicketReply(int ticketId, string message)
        {
            var ticket = await _context.SupportTickets
                .Include(st => st.Messages)
                .FirstOrDefaultAsync(st => st.Id == ticketId);

            if (ticket == null)
            {
                TempData["Error"] = "Destek talebi bulunamadı.";
                return RedirectToAction(nameof(SupportTickets));
            }

            var supportMessage = new SupportMessage
            {
                SupportTicketId = ticketId,
                SupportTicket = ticket,
                Message = message,
                CreatedAt = DateTime.Now,
                IsActive = true,
                IsStaffReply = true
            };

            _context.Add(supportMessage);
            ticket.Status = "Answered";
            ticket.LastUpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Yanıtınız başarıyla gönderildi.";
            return RedirectToAction(nameof(SupportTicketDetails), new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTicketStatus(int ticketId, string status)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);

            if (ticket == null)
            {
                TempData["Error"] = "Destek talebi bulunamadı.";
                return RedirectToAction(nameof(SupportTickets));
            }

            ticket.Status = status;
            ticket.LastUpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Destek talebi durumu başarıyla güncellendi.";
            return RedirectToAction(nameof(SupportTicketDetails), new { id = ticketId });
        }

        public async Task<IActionResult> Coupons()
        {
            var coupons = await _context.Coupons
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(coupons);
        }

        public IActionResult CreateCoupon()
        {
            return View(new Coupon { 
                Code = "", 
                DiscountType = "Fixed",
                CreatedAt = DateTime.Now,
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCoupon(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Coupons.AnyAsync(c => c.Code == coupon.Code))
                {
                    ModelState.AddModelError("Code", "Bu kupon kodu zaten kullanılıyor.");
                    return View(coupon);
                }

                coupon.CreatedAt = DateTime.Now;
                coupon.IsActive = true;
                
                _context.Add(coupon);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Kupon başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Coupons));
            }
            return View(coupon);
        }

        public async Task<IActionResult> EditCoupon(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCoupon(int id, Coupon coupon)
        {
            if (id != coupon.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCoupon = await _context.Coupons.FindAsync(id);
                    if (existingCoupon == null)
                    {
                        return NotFound();
                    }

                    if (existingCoupon.Code != coupon.Code && 
                        await _context.Coupons.AnyAsync(c => c.Code == coupon.Code))
                    {
                        ModelState.AddModelError("Code", "Bu kupon kodu zaten kullanılıyor.");
                        return View(coupon);
                    }

                    existingCoupon.Code = coupon.Code;
                    existingCoupon.DiscountAmount = coupon.DiscountAmount;
                    existingCoupon.DiscountType = coupon.DiscountType;
                    existingCoupon.MinimumCartAmount = coupon.MinimumCartAmount;
                    existingCoupon.StartDate = coupon.StartDate;
                    existingCoupon.EndDate = coupon.EndDate;
                    existingCoupon.IsActive = coupon.IsActive;
                    existingCoupon.UsageLimit = coupon.UsageLimit;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Kupon başarıyla güncellendi.";
                    return RedirectToAction(nameof(Coupons));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CouponExists(coupon.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCouponStatus(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            coupon.IsActive = !coupon.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Kupon durumu {(coupon.IsActive ? "aktif" : "pasif")} olarak güncellendi.";
            return RedirectToAction(nameof(Coupons));
        }

        private bool CouponExists(int id)
        {
            return _context.Coupons.Any(e => e.Id == id);
        }
    }
} 