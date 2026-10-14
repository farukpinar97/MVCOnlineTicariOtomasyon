using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.ViewModels;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    [AdminAuthorize]   // sadece yönetici girişi yapanlar
    public class ProductDetailController : Controller
    {
        Context c = new Context();

        // Bir ürünün detay sayfası
        public ActionResult Index(int? id)
        {
            int productId = id ?? c.Products
                .Where(x => x.Status == true)
                .OrderBy(x => x.ProductId)
                .Select(x => x.ProductId)
                .FirstOrDefault();

            var products = c.Products
                .Include(x => x.Category)
                .Where(x => x.ProductId == productId)
                .ToList();

            if (products.Count == 0)
            {
                // Numara verildiyse öyle bir ürün yok
                return id.HasValue ? (ActionResult)HttpNotFound() : RedirectToAction("Index", "Product");
            }

            string productName = products[0].ProductName;
            var sales = c.SalesTransactions.Where(x => x.ProductId == productId);

            var model = new ProductDetailViewModel();
            model.Products = products;
            model.Details = c.Details
                .Where(x => x.ProductName == productName)
                .OrderBy(x => x.DetailId)
                .ToList();
            model.RecentSales = sales
                .Include(x => x.Current)
                .Include(x => x.Personnel)
                .OrderByDescending(x => x.SalesTransactionDate)
                .ThenByDescending(x => x.SalesTransactionId)
                .Take(10)
                .ToList();

            model.TotalSold = sales.Sum(x => (int?)x.SalesTransactionQuantity) ?? 0;
            model.TotalRevenue = sales.Sum(x => (decimal?)x.SalesTransactionTotalPrice) ?? 0;

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
