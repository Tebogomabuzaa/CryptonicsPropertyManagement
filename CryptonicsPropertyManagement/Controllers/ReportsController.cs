using CryptonicsPropertyManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService) => _reportService = reportService;

        public IActionResult CoHostPerformance() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CoHostPerformance(DateTime startDate, DateTime endDate, string periodType)
        {
            if (periodType == "monthly")
            {
                startDate = new DateTime(startDate.Year, startDate.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }
            else if (periodType == "quarterly")
            {
                int quarter = (startDate.Month - 1) / 3;
                startDate = new DateTime(startDate.Year, quarter * 3 + 1, 1);
                endDate = startDate.AddMonths(3).AddDays(-1);
            }

            if (endDate < startDate)
            {
                ModelState.AddModelError(string.Empty, "End date must be on or after start date.");
                return View();
            }

            var data = await _reportService.GetCoHostPerformanceAsync(startDate, endDate);

            var labels = new List<string>();
            var rates = new List<decimal>();
            var colors = new List<string>();

            foreach (var item in data)
            {
                labels.Add(item.ManagerName);
                rates.Add(item.OccupancyRate);
                colors.Add(item.OccupancyRate >= 70 ? "#28a745" : "#dc3545");
            }

            ViewBag.ChartJson = JsonConvert.SerializeObject(new { labels, rates, colors });
            ViewBag.StartDate = startDate.ToString("dd MMM yyyy");
            ViewBag.EndDate = endDate.ToString("dd MMM yyyy");
            ViewBag.PeriodType = periodType;

            return View(data);
        }
    }
}
