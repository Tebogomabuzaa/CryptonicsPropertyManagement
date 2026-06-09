using CryptonicsPropertyManagement.Models.Entities;
using CryptonicsPropertyManagement.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Controllers
{
    public class ManagersController : Controller
    {
        private readonly ManagerRepository _managerRepo;

        public ManagersController(ManagerRepository managerRepo) => _managerRepo = managerRepo;

        public async Task<IActionResult> Index() => View(await _managerRepo.GetAllAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyManager manager)
        {
            if (!ModelState.IsValid) return View(manager);

            int newId = await _managerRepo.AddAsync(manager);
            TempData["Success"] = $"Property manager added. ID: {newId}";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var manager = await _managerRepo.GetByIdAsync(id);
            if (manager == null) return NotFound();
            return View(manager);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PropertyManager manager)
        {
            if (!ModelState.IsValid) return View(manager);

            await _managerRepo.UpdateAsync(manager);
            TempData["Success"] = $"Manager ID {manager.ManagerID} successfully updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var manager = await _managerRepo.GetByIdAsync(id);
            if (manager == null) return NotFound();
            return View(manager);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _managerRepo.DeleteAsync(id);
            TempData["Success"] = $"Manager ID {id} successfully deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
