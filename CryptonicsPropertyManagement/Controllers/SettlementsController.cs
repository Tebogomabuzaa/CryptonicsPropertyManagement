using CryptonicsPropertyManagement.Models.Entities;
using CryptonicsPropertyManagement.Repositories;
using CryptonicsPropertyManagement.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Controllers
{
    public class SettlementsController : Controller
    {
        private readonly SettlementRepository _settlementRepo;
        private readonly LeaseRepository _leaseRepo;
        private readonly SettlementService _settlementService;
        private readonly CryptoInvoiceService _cryptoService;

        public SettlementsController(
            SettlementRepository settlementRepo,
            LeaseRepository leaseRepo,
            SettlementService settlementService,
            CryptoInvoiceService cryptoService)
        {
            _settlementRepo = settlementRepo;
            _leaseRepo = leaseRepo;
            _settlementService = settlementService;
            _cryptoService = cryptoService;
        }

        public async Task<IActionResult> Index() => View(await _settlementRepo.GetAllAsync());

        public async Task<IActionResult> Create()
        {
            ViewBag.Leases = await _leaseRepo.GetAllAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int leaseId, decimal monthlyRent, decimal maintenanceCosts, int daysOccupied)
        {
            try
            {
                var result = _settlementService.CalculatePayout(monthlyRent, maintenanceCosts, daysOccupied);
                string invoiceLink = await _cryptoService.GenerateInvoiceLinkAsync(leaseId, result.OwnerPayout);

                var settlement = new Settlement
                {
                    LeaseID = leaseId,
                    GrossRent = result.GrossRent,
                    MaintenanceCosts = result.MaintenanceCosts,
                    NetAmount = result.NetAmount,
                    ManagementFee = result.ManagementFee,
                    OwnerPayout = result.OwnerPayout,
                    DaysOccupied = result.DaysOccupied,
                    SettlementDate = DateTime.Now,
                    CryptoInvoiceLink = invoiceLink
                };

                int newId = await _settlementRepo.AddAsync(settlement);
                TempData["Success"] = $"Settlement ID {newId} created. Owner payout: R{result.OwnerPayout:N2}. Notification sent to owner and clerk.";
                TempData["InvoiceLink"] = invoiceLink;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Leases = await _leaseRepo.GetAllAsync();
                return View();
            }
        }
    }
}
