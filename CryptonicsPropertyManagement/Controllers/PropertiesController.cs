// Controllers/PropertiesController.cs
using CryptonicsPropertyManagement.Models.Entities;
using CryptonicsPropertyManagement.Repositories;
using Microsoft.AspNetCore.Mvc;
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

        // GET: /Properties - Loads the main dashboard table
        public async Task<IActionResult> Index()
        {
            var properties = await _propertyRepo.GetAllAsync();
            return View(properties);
        }

        // GET: /Properties/Create - Loads the blank form to add a new property
        public async Task<IActionResult> Create()
        {
            // We pass the list of owners to the View via ViewBag for the dropdown
            ViewBag.Owners = await _ownerRepo.GetAllAsync();
            return View();
        }

        // POST: /Properties/Create - Handles the form submission when the user clicks "Save"
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
            catch (System.InvalidOperationException ex)
            {
                // This catches the duplicate address error we wrote in the repository!
                ModelState.AddModelError("PhysicalAddress", ex.Message);
                ViewBag.Owners = await _ownerRepo.GetAllAsync();
                return View(property);
            }
        }
    }
}