using CryptonicsPropertyManagement.Models.Entities;
using CryptonicsPropertyManagement.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Controllers
{
    public class LeasesController : Controller
    {
        private readonly LeaseRepository _leaseRepo;
        private readonly PropertyRepository _propertyRepo;
        private readonly TenantRepository _tenantRepo;
        private readonly ManagerRepository _managerRepo;

        public LeasesController(
            LeaseRepository leaseRepo,
            PropertyRepository propertyRepo,
            TenantRepository tenantRepo,
            ManagerRepository managerRepo)
        {
            _leaseRepo = leaseRepo;
            _propertyRepo = propertyRepo;
            _tenantRepo = tenantRepo;
            _managerRepo = managerRepo;
        }

        public async Task<IActionResult> Index() => View(await _leaseRepo.GetAllAsync());

        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaseAgreement lease)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(lease);
            }

            int newId = await _leaseRepo.AddAsync(lease);
            TempData["Success"] = $"Lease agreement created. Lease ID: {newId}";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var lease = await _leaseRepo.GetByIdAsync(id);
            if (lease == null) return NotFound();
            await LoadDropdowns();
            return View(lease);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LeaseAgreement lease)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(lease);
            }

            await _leaseRepo.UpdateAsync(lease);
            TempData["Success"] = $"Lease ID {lease.LeaseID} successfully updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var lease = await _leaseRepo.GetByIdAsync(id);
            if (lease == null) return NotFound();
            return View(lease);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _leaseRepo.DeleteAsync(id);
            TempData["Success"] = $"Lease ID {id} successfully deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns()
        {
            ViewBag.Properties = await _propertyRepo.GetAllAsync();
            ViewBag.Tenants = await _tenantRepo.GetAllAsync();
            ViewBag.Managers = await _managerRepo.GetAllAsync();
        }
    }
}
