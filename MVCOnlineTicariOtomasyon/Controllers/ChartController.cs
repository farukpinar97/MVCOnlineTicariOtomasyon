using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    // Grafikler sayfası
    [AdminAuthorize]   // sadece yönetici girişi yapanlar
    public class ChartController : Controller
    {
        Context c = new Context();

        // Seçilebilecek dönemler
        private static readonly int[] Periods = { 7, 30, 90, 365 };

        private static readonly CultureInfo Tr = new CultureInfo("tr-TR");

        // Kategori grafiğinde en fazla bu kadar dilim
        private const int MaxCategories = 5;

        public ActionResult Index(int days = 30)
        {
            ViewBag.Days = Periods.Contains(days) ? days : 30;
            ViewBag.Periods = Periods;
            return View();
        }

        public JsonResult SalesData(int days = 30)
        {
            if (!Periods.Contains(days))
            {
                days = 30;
            }

            bool monthly = days > 90;
            DateTime end = DateTime.Today.AddDays(1);   // yarın 00:00
            DateTime start;
            if (monthly)
            {
                // Son 12 ay
                start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-11);
            }
            else
            {
                start = end.AddDays(-days);
            }
            TimeSpan length = end - start;
            DateTime previousStart = monthly ? start.AddMonths(-12) : start - length;

            // İki dönemin satırları tek sorguda
            var rows = c.SalesTransactions
                .Where(x => x.SalesTransactionDate >= previousStart && x.SalesTransactionDate < end)
                .Select(x => new
                {
                    Date = x.SalesTransactionDate,
                    Quantity = x.SalesTransactionQuantity,
                    Total = x.SalesTransactionTotalPrice,
                    Product = x.Product.ProductName,
                    Category = x.Product.Category.CategoryName,
                    PersonnelName = x.Personnel.PersonnelName,
                    PersonnelSurname = x.Personnel.PersonnelSurname
                })
                .ToList();

            var current = rows.Where(x => x.Date >= start).ToList();
            var previous = rows.Where(x => x.Date < start).ToList();

            // Çizgi grafik
            var points = new List<object>();
            int pointCount = monthly ? 12 : days;
            for (int i = 0; i < pointCount; i++)
            {
                DateTime from = monthly ? start.AddMonths(i) : start.AddDays(i);
                DateTime to = monthly ? from.AddMonths(1) : from.AddDays(1);
                DateTime prevFrom = monthly ? previousStart.AddMonths(i) : previousStart.AddDays(i);
                DateTime prevTo = monthly ? prevFrom.AddMonths(1) : prevFrom.AddDays(1);

                points.Add(new
                {
                    label = monthly ? from.ToString("MMM yy", Tr) : from.ToString("d MMM", Tr),
                    title = monthly ? from.ToString("MMMM yyyy", Tr) : from.ToString("d MMMM yyyy, dddd", Tr),
                    previousTitle = monthly ? prevFrom.ToString("MMMM yyyy", Tr) : prevFrom.ToString("d MMMM yyyy", Tr),
                    current = current.Where(x => x.Date >= from && x.Date < to).Sum(x => x.Total),
                    previous = previous.Where(x => x.Date >= prevFrom && x.Date < prevTo).Sum(x => x.Total)
                });
            }

            // Yatay çubuk
            var topProducts = current
                .GroupBy(x => x.Product)
                .Select(g => new { name = g.Key, revenue = g.Sum(x => x.Total), quantity = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.revenue)
                .Take(7)
                .ToList();

            // Halka
            var categoryTotals = current
                .GroupBy(x => x.Category)
                .Select(g => new { name = g.Key, revenue = g.Sum(x => x.Total) })
                .OrderByDescending(x => x.revenue)
                .ToList();
            var categories = categoryTotals.Take(MaxCategories)
                .Select(x => new { x.name, x.revenue, other = false })
                .ToList();
            if (categoryTotals.Count > MaxCategories)
            {
                categories.Add(new
                {
                    name = "Diğer (" + (categoryTotals.Count - MaxCategories) + " kategori)",
                    revenue = categoryTotals.Skip(MaxCategories).Sum(x => x.revenue),
                    other = true
                });
            }

            // Sütun
            var personnel = current
                .GroupBy(x => x.PersonnelName + " " + x.PersonnelSurname)
                .Select(g => new { name = g.Key, revenue = g.Sum(x => x.Total), count = g.Count() })
                .OrderByDescending(x => x.revenue)
                .ToList();

            var result = new
            {
                days,
                monthly,
                periodLabel = start.ToString("d MMMM yyyy", Tr) + " – " + end.AddDays(-1).ToString("d MMMM yyyy", Tr),
                compareLabel = monthly ? "önceki 12 aya göre" : "önceki " + days + " güne göre",
                tiles = new
                {
                    revenue = Pair(current.Sum(x => x.Total), previous.Sum(x => x.Total)),
                    count = Pair(current.Count, previous.Count),
                    quantity = Pair(current.Sum(x => x.Quantity), previous.Sum(x => x.Quantity)),
                    average = Pair(
                        current.Count == 0 ? 0 : current.Sum(x => x.Total) / current.Count,
                        previous.Count == 0 ? 0 : previous.Sum(x => x.Total) / previous.Count)
                },
                points,
                topProducts,
                categories,
                personnel
            };

            // AllowGet
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Kart değeri
        private static object Pair(decimal value, decimal previous)
        {
            decimal? change = previous == 0 ? (decimal?)null : Math.Round((value - previous) / previous * 100, 1);
            return new { value, previous, change };
        }

        // İstek bitince veritabanı bağlantısını serbest bırak
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                c.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
