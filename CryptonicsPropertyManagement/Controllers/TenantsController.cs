using CryptonicsPropertyManagement.Models.Entities;
using CryptonicsPropertyManagement.Repositories;
using CryptonicsPropertyManagement.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index() => View(await _tenantRepo.GetAllAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tenant tenant)
        {
            if (!ModelState.IsValid) return View(tenant);

            try
            {
                tenant.VerificationStatus = "Pending";
                int newId = await _tenantRepo.AddAsync(tenant);
                TempData["Success"] = $"New tenant registered. Tenant ID: {newId}. Awaiting KYC verification.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("PassportIDNumber", ex.Message);
                return View(tenant);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var tenant = await _tenantRepo.GetByIdAsync(id);
            if (tenant == null) return NotFound();
            return View(tenant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tenant tenant)
        {
            if (!ModelState.IsValid) return View(tenant);

            await _tenantRepo.UpdateAsync(tenant);
            TempData["Success"] = $"Tenant ID {tenant.TenantID} successfully updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var tenant = await _tenantRepo.GetByIdAsync(id);
            if (tenant == null) return NotFound();
            return View(tenant);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _tenantRepo.DeleteAsync(id);
            TempData["Success"] = $"Tenant ID {id} successfully deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunKyc(int tenantId, string passport, decimal income)
        {
            string resultStatus = await _kycService.VerifyTenantAsync(passport, income);
            await _tenantRepo.UpdateKycStatusAsync(tenantId, resultStatus);

            TempData["Success"] = $"Automated KYC Check Complete: {resultStatus}";
            return RedirectToAction(nameof(Index));
        }
    }
}
