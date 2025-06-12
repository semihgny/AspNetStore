using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnet.Data;
using aspnet.Models;
using Microsoft.AspNetCore.Authorization;

namespace aspnet.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CouponController : Controller
    {
        private readonly DataContext _context;
        private readonly ILogger<CouponController> _logger;

        public CouponController(DataContext context, ILogger<CouponController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var coupons = await _context.Coupons
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(coupons);
        }

        public IActionResult Create()
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
        public async Task<IActionResult> Create(Coupon coupon)
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
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        public async Task<IActionResult> Edit(int? id)
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
        public async Task<IActionResult> Edit(int id, Coupon coupon)
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
                    return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            coupon.IsActive = !coupon.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Kupon durumu {(coupon.IsActive ? "aktif" : "pasif")} olarak güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        private bool CouponExists(int id)
        {
            return _context.Coupons.Any(e => e.Id == id);
        }
    }
} 