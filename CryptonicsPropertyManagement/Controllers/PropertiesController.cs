using CryptonicsPropertyManagement.Models.Entities;
using CryptonicsPropertyManagement.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly PropertyRepository _propertyRepo;
        private readonly OwnerRepository _ownerRepo;

        public PropertiesController(PropertyRepository propertyRepo, OwnerRepository ownerRepo)
        {
            _propertyRepo = propertyRepo;
            _ownerRepo = ownerRepo;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _propertyRepo.GetAllAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Owners = await _ownerRepo.GetAllAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Property property)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Owners = await _ownerRepo.GetAllAsync();
                return View(property);
            }

            try
            {
                int newId = await _propertyRepo.AddAsync(property);
                TempData["Success"] = $"Property successfully added. ID: {newId}";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("PhysicalAddress", ex.Message);
                ViewBag.Owners = await _ownerRepo.GetAllAsync();
                return View(property);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var property = await _propertyRepo.GetByIdAsync(id);
            if (property == null) return NotFound();
            ViewBag.Owners = await _ownerRepo.GetAllAsync();
            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Property property)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Owners = await _ownerRepo.GetAllAsync();
                return View(property);
            }

            await _propertyRepo.UpdateAsync(property);
            TempData["Success"] = $"Property ID {property.PropertyID} successfully updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var property = await _propertyRepo.GetByIdAsync(id);
            if (property == null) return NotFound();
            return View(property);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _propertyRepo.DeleteAsync(id);
            TempData["Success"] = $"Property ID {id} successfully deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
