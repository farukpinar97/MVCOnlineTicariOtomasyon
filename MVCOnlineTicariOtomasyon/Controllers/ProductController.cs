using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.Entities;
using PagedList;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    [AdminAuthorize]   // sadece yönetici girişi yapanlar
    public class ProductController : Controller
    {
        Context c = new Context();

        // Listede bir sayfada kaç ürün görünsün
        private const int PageSize = 10;

        // Stoku bu sayıya eşit ya da altında olan ürün "Azaldı" diye işaretlenir
        private const int LowStock = 10;

        // Aktif ürünler
        public ActionResult Index(string q, int page = 1)
        {
            q = Clean(q);

            var active = c.Products.Where(x => x.Status == true);

            var query = active.Include(x => x.Category);
            if (q != null)
            {
                query = query.Where(x => x.ProductName.Contains(q)
                                      || x.Brand.Contains(q)
                                      || x.Category.CategoryName.Contains(q));
            }

            // Sayfalama için sıralama şart
            var products = query.OrderBy(x => x.ProductId).ToPagedList(Math.Max(page, 1), PageSize);

            // Olmayan bir sayfa istendiyse son sayfaya git
            if (page > 1 && page > products.PageCount)
            {
                return RedirectToAction("Index", new { q, page = products.PageCount > 1 ? products.PageCount : (int?)null });
            }

            ViewBag.TotalStock = active.Sum(x => (int?)x.Stock) ?? 0;
            ViewBag.PurchaseValue = active.Sum(x => (decimal?)(x.Stock * x.PurchasePrice)) ?? 0m;
            ViewBag.SaleValue = active.Sum(x => (decimal?)(x.Stock * x.SalePrice)) ?? 0m;
            ViewBag.LowCount = active.Count(x => x.Stock <= LowStock);
            ViewBag.ActiveCount = active.Count();
            ViewBag.LowStock = LowStock;
            ViewBag.Query = q;

            return View(products);
        }

        // Pasifler dahil bütün ürünler
        public ActionResult ProductList()
        {
            var products = c.Products.Include(x => x.Category).ToList();
            return View(products);
        }

        // Ekleme formu
        public ActionResult ProductAdd()
        {
            FillCategories();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductAdd(Product p)
        {
            ValidateProduct(p);
            if (!ModelState.IsValid)
            {
                FillCategories();
                return View(p);
            }

            p.Status = true;   // yeni ürün aktif başlar
            c.Products.Add(p);
            c.SaveChanges();

            TempData["Success"] = $"“{p.ProductName}” eklendi.";
            return RedirectToAction("Index");
        }

        // Silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        // page ve q, silme butonunun adresinden gelir
        public ActionResult ProductDelete(int id, int page = 1, string q = null)
        {
            var product = c.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            product.Status = false;
            c.SaveChanges();

            TempData["Success"] = $"“{product.ProductName}” silindi.";
            return RedirectToAction("Index", new { q = Clean(q), page = page > 1 ? page : (int?)null });
        }

        // Güncelleme formu
        public ActionResult ProductGet(int id)
        {
            var product = c.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            FillCategories();
            return View("ProductGet", product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductUpdate(Product p)
        {
            var product = c.Products.Find(p.ProductId);
            if (product == null)
            {
                return HttpNotFound();
            }

            ValidateProduct(p);
            if (!ModelState.IsValid)
            {
                FillCategories();
                return View("ProductGet", p);
            }

            product.ProductName = p.ProductName;
            product.Brand = p.Brand;
            product.CategoryId = p.CategoryId;
            product.Stock = p.Stock;
            product.PurchasePrice = p.PurchasePrice;
            product.SalePrice = p.SalePrice;
            product.ProductImage = p.ProductImage;
            product.Status = p.Status;
            c.SaveChanges();

            TempData["Success"] = $"“{product.ProductName}” güncellendi.";
            // Pasife alınan ürün Index'te görünmez
            return RedirectToAction(product.Status ? "Index" : "ProductList");
        }

        // Formdaki kategori açılır listesi
        private void FillCategories()
        {
            ViewBag.Categories = c.Categories
                .OrderBy(x => x.CategoryName)
                .ToList()
                .Select(x => new SelectListItem { Text = x.CategoryName, Value = x.CategoryId.ToString() })
                .ToList();
        }

        // Ekleme ve güncellemede ortak kontroller
        private void ValidateProduct(Product p)
        {
            p.ProductName = (p.ProductName ?? "").Trim();
            p.Brand = string.IsNullOrWhiteSpace(p.Brand) ? null : p.Brand.Trim();
            p.ProductImage = string.IsNullOrWhiteSpace(p.ProductImage) ? null : p.ProductImage.Trim();

            if (p.ProductName == "")
            {
                ModelState.AddModelError("ProductName", "Ürün adı boş bırakılamaz.");
            }

            int categoryId = p.CategoryId;
            if (!ReplaceBindingError("CategoryId", "Bir kategori seç.") && !c.Categories.Any(x => x.CategoryId == categoryId))
            {
                ModelState.AddModelError("CategoryId", "Bir kategori seç.");
            }

            if (!ReplaceBindingError("Stock", "Stok için 0 ile 32767 arasında tam sayı yaz.") && p.Stock < 0)
            {
                ModelState.AddModelError("Stock", "Stok eksi olamaz.");
            }

            // Örnekteki ondalık ayracı sunucunun bölge ayarından gelir
            string priceFormat = "Fiyatı sayı olarak yaz. Örnek: " + (1250.5m).ToString("0.00");

            if (!ReplaceBindingError("PurchasePrice", priceFormat) && p.PurchasePrice < 0)
            {
                ModelState.AddModelError("PurchasePrice", "Alış fiyatı eksi olamaz.");
            }

            if (!ReplaceBindingError("SalePrice", priceFormat) && p.SalePrice <= 0)
            {
                ModelState.AddModelError("SalePrice", "Satış fiyatı 0'dan büyük olmalı.");
            }
        }

        // Formdan gelen değer sayıya çevrilemediyse MVC kendi İngilizce mesajını ekler
        private bool ReplaceBindingError(string key, string message)
        {
            if (ModelState.IsValidField(key))
            {
                return false;
            }

            ModelState[key].Errors.Clear();
            ModelState.AddModelError(key, message);
            return true;
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
