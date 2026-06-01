// Controllers/TenantsController.cs
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Models.Entities;
using CryptonicsPropertyManagement.Repositories;
using CryptonicsPropertyManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptonicsPropertyManagement.Controllers
{
    public class TenantsController : Controller
    {
        private readonly TenantRepository _tenantRepo;
        private readonly KycService _kycService;

        public TenantsController(TenantRepository tenantRepo, KycService kycService)
        {
            _tenantRepo = tenantRepo;
            _kycService = kycService;
        }

        // GET: /Tenants
        public async Task<IActionResult> Index()
        {
            var tenants = await _tenantRepo.GetAllAsync();
            return View(tenants);
        }

        // GET: /Tenants/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Tenants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tenant tenant)
        {
            if (!ModelState.IsValid) return View(tenant);

            await _tenantRepo.AddAsync(tenant);
            TempData["Success"] = "New tenant successfully registered and is awaiting KYC verification.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Tenants/RunKyc
        // This triggers your Mock API business logic!
        [HttpPost]
        public async Task<IActionResult> RunKyc(int tenantId, string passport, decimal income)
        {
            // 1. Run the simulated API check (pauses for 2 seconds automatically)
            string resultStatus = await _kycService.VerifyTenantAsync(passport, income);

            // 2. Safely update only the verification field in MS Access
            await _tenantRepo.UpdateKycStatusAsync(tenantId, resultStatus);

            // 3. Return to the dashboard with the result
            TempData["Success"] = $"Automated KYC Check Complete: {resultStatus}";
            return RedirectToAction(nameof(Index));
        }
    }
}