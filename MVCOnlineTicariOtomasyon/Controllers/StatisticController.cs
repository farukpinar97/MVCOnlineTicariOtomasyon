using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    [AdminAuthorize]   // sadece yönetici girişi yapanlar
    public class StatisticController : Controller
    {
        Context c = new Context();

        // Stoku bu sayıya eşit ya da altında olan ürün "azalan" sayılır
        const int LowStock = 10;

        public ActionResult Index()
        {
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);
            DateTime weekStart = today.AddDays(-6);   // bugün dahil son 7 gün

            // Ürün rakamlarında sadece aktif ürünler sayılır
            var products = c.Products.Where(x => x.Status == true);
            var todaySales = c.SalesTransactions.Where(x => x.SalesTransactionDate >= today && x.SalesTransactionDate < tomorrow);

            var model = new StatisticViewModel();
            model.Today = today;
            model.LowStockLimit = LowStock;

            // Satış
            model.TodayRevenue = todaySales.Sum(x => (decimal?)x.SalesTransactionTotalPrice) ?? 0;
            model.TodaySalesCount = todaySales.Count();
            model.TotalRevenue = c.SalesTransactions.Sum(x => (decimal?)x.SalesTransactionTotalPrice) ?? 0;
            model.TotalSalesCount = c.SalesTransactions.Count();

            // En çok satan
            var bestSeller = c.SalesTransactions
                .GroupBy(x => x.Product.ProductName)
                .Select(g => new { Name = g.Key, Quantity = g.Sum(x => x.SalesTransactionQuantity) })
                .OrderByDescending(x => x.Quantity)
                .FirstOrDefault();
            if (bestSeller != null)
            {
                model.BestSellerName = bestSeller.Name;
                model.BestSellerQuantity = bestSeller.Quantity;
            }

            // Grafik
            var recent = c.SalesTransactions
                .Where(x => x.SalesTransactionDate >= weekStart && x.SalesTransactionDate < tomorrow)
                .Select(x => new { x.SalesTransactionDate, x.SalesTransactionTotalPrice })
                .ToList();
            model.Last7Days = Enumerable.Range(0, 7)
                .Select(i => weekStart.AddDays(i))
                .Select(day => new DailySales
                {
                    Date = day,
                    Revenue = recent.Where(x => x.SalesTransactionDate.Date == day).Sum(x => x.SalesTransactionTotalPrice),
                    Count = recent.Count(x => x.SalesTransactionDate.Date == day)
                })
                .ToList();

            // Stok ve ürünler
            model.ProductCount = products.Count();
            model.TotalStock = products.Sum(x => (int?)x.Stock) ?? 0;
            model.LowStockCount = products.Count(x => x.Stock <= LowStock);

            var branded = products.Where(x => x.Brand != null && x.Brand != "");
            model.BrandCount = branded.Select(x => x.Brand).Distinct().Count();

            var topBrand = branded
                .GroupBy(x => x.Brand)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();
            if (topBrand != null)
            {
                model.TopBrandName = topBrand.Name;
                model.TopBrandProductCount = topBrand.Count;
            }

            var mostExpensive = products.OrderByDescending(x => x.SalePrice).FirstOrDefault();
            if (mostExpensive != null)
            {
                model.MostExpensiveName = mostExpensive.ProductName;
                model.MostExpensivePrice = mostExpensive.SalePrice;
            }

            var cheapest = products.OrderBy(x => x.SalePrice).FirstOrDefault();
            if (cheapest != null)
            {
                model.CheapestName = cheapest.ProductName;
                model.CheapestPrice = cheapest.SalePrice;
            }

            // Grafik
            model.CategoryRows = c.Categories
                .Select(x => new CategoryProductCount
                {
                    Name = x.CategoryName,
                    ProductCount = x.Products.Count(p => p.Status == true)
                })
                .Where(x => x.ProductCount > 0)
                .OrderByDescending(x => x.ProductCount)
                .ThenBy(x => x.Name)
                .Take(8)
                .ToList();

            // Genel
            model.CurrentCount = c.Currents.Count(x => x.Status == true);
            model.PersonnelCount = c.Personnels.Count();
            model.CategoryCount = c.Categories.Count();
            model.DepartmentCount = c.Departments.Count(x => x.Status == true);

            return View(model);
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
