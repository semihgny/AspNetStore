using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnet.Models;
using aspnet.Data;

namespace aspnet.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DataContext _context;

    public HomeController(ILogger<HomeController> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            if (categories == null || !categories.Any())
            {
                return View(new List<Product>());
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Category != null && p.Category.IsActive)
                .ToListAsync();

            if (products == null || !products.Any())
            {
                return View(new List<Product>());
            }

            foreach (var category in categories)
            {
                category.Products = products
                    .Where(p => p.CategoryId == category.Id)
                    .ToList();
            }

            categories = categories.Where(c => c.Products != null && c.Products.Any()).ToList();

            ViewBag.Categories = categories;
            return View(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Anasayfada ürünler listelenirken hata oluştu");
            TempData["Error"] = "Ürünler listelenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
            return View(new List<Product>());
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
