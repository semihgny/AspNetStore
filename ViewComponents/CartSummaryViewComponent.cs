using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnet.Data;
using aspnet.Models;

namespace aspnet.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly DataContext _context;

        public CartSummaryViewComponent(DataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cartId = HttpContext.Session.GetInt32("CartId");
            if (cartId == null)
            {
                return View(0);
            }

            var itemCount = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .SumAsync(ci => ci.Quantity);

            return View(itemCount);
        }
    }
} 