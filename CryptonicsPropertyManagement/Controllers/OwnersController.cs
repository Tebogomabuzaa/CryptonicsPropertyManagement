using CryptonicsPropertyManagement.Models.Entities;
using CryptonicsPropertyManagement.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Controllers
{
    public class OwnersController : Controller
    {
        private readonly OwnerRepository _ownerRepo;

        public OwnersController(OwnerRepository ownerRepo) => _ownerRepo = ownerRepo;

        public async Task<IActionResult> Index() => View(await _ownerRepo.GetAllAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Owner owner)
        {
            if (!ModelState.IsValid) return View(owner);

            int newId = await _ownerRepo.AddAsync(owner);
            TempData["Success"] = $"Owner successfully added. ID: {newId}";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var owner = await _ownerRepo.GetByIdAsync(id);
            if (owner == null) return NotFound();
            return View(owner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Owner owner)
        {
            if (!ModelState.IsValid) return View(owner);

            await _ownerRepo.UpdateAsync(owner);
            TempData["Success"] = $"Owner ID {owner.OwnerID} successfully updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var owner = await _ownerRepo.GetByIdAsync(id);
            if (owner == null) return NotFound();
            return View(owner);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _ownerRepo.DeleteAsync(id);
            TempData["Success"] = $"Owner ID {id} successfully deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
