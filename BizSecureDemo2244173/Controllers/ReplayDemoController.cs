using System.Security.Claims;
using BizSecureDemo.Data;
using BizSecureDemo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BizSecureDemo.Controllers
{
    [Authorize]
    public class ReplayDemoController : Controller
    {
        private static decimal _balance = 1000m;
        private static readonly HashSet<string> _usedNonces = new();
        private readonly AppDbContext _db;

        public ReplayDemoController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var vm = new ReplayDemoVm
            {
                Balance = _balance,
                Message = TempData["Message"]?.ToString(),
                UserId = userId,
                Nonce = Guid.NewGuid().ToString()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Withdraw(ReplayDemoVm vm)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            if (vm.Token != "SECRET123")
            {
                TempData["Message"] = "Invalid token.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(vm.Nonce))
            {
                TempData["Message"] = "Missing nonce.";
                return RedirectToAction(nameof(Index));
            }

            if (_usedNonces.Contains(vm.Nonce))
            {
                TempData["Message"] = "Replay attack detected. Request already used.";
                return RedirectToAction(nameof(Index));
            }

            _usedNonces.Add(vm.Nonce);

            if (vm.Amount <= 0)
            {
                TempData["Message"] = "Amount must be greater than 0.";
                return RedirectToAction(nameof(Index));
            }

            if (_balance < vm.Amount)
            {
                TempData["Message"] = "Insufficient balance.";
                return RedirectToAction(nameof(Index));
            }

            _balance -= vm.Amount;
            TempData["Message"] = $"Withdrawal successful. User: {userId}, Amount: {vm.Amount}, Remaining balance: {_balance}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reset()
        {
            _balance = 1000m;
            TempData["Message"] = $"Balance reset. Current balance: {_balance}";
            return RedirectToAction(nameof(Index));
        }
    }
}