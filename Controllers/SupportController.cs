using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnet.Data;
using aspnet.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace aspnet.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        private readonly DataContext _context;
        private readonly ILogger<SupportController> _logger;

        public SupportController(DataContext context, ILogger<SupportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            var tickets = await _context.SupportTickets
                .Include(st => st.Messages)
                .Where(st => isAdmin || st.UserId == userId)
                .OrderByDescending(st => st.CreatedAt)
                .ToListAsync();

            return View(tickets);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            var ticket = await _context.SupportTickets
                .Include(st => st.Messages)
                .FirstOrDefaultAsync(st => st.Id == id && (isAdmin || st.UserId == userId));

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupportTicket ticket)
        {
            try
            {
                                if (string.IsNullOrEmpty(ticket.Subject) || string.IsNullOrEmpty(ticket.Message))
                {
                    ModelState.Clear();
                    ModelState.AddModelError("", "Konu ve mesaj alanları zorunludur.");
                    return View(ticket);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FindAsync(int.Parse(userId ?? "0"));

                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                ticket.TicketNumber = GenerateTicketNumber();
                ticket.UserId = user.Id;
                ticket.Name = $"{user.FirstName} {user.LastName}";
                ticket.Email = user.Email;
                ticket.Status = "Open";
                ticket.CreatedAt = DateTime.Now;
                ticket.IsActive = true;

                _context.Add(ticket);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Destek talebiniz başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Destek talebi oluşturulurken hata: {ex.Message}");
                ModelState.AddModelError("", "Destek talebi oluşturulurken bir hata oluştu.");
                return View(ticket);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int ticketId, string message)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            var ticket = await _context.SupportTickets
                .FirstOrDefaultAsync(st => st.Id == ticketId && (isAdmin || st.UserId == userId));

            if (ticket == null)
            {
                return NotFound();
            }

            var supportMessage = new SupportMessage
            {
                SupportTicketId = ticketId,
                SupportTicket = ticket,
                Message = message,
                CreatedAt = DateTime.Now,
                IsActive = true,
                IsStaffReply = isAdmin
            };

            _context.Add(supportMessage);

            ticket.LastUpdatedAt = DateTime.Now;
            ticket.Status = isAdmin ? "Answered" : "Waiting";
            _context.Update(ticket);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Status = status;
            ticket.LastUpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Destek talebi durumu güncellendi.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        private string GenerateTicketNumber()
        {
            return $"TKT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
} 