using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.Entities;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    [AdminAuthorize]   // sadece yönetici girişi yapanlar
    public class SalesController : Controller
    {
        Context c = new Context();

        // Bütün satışlar, yeniden eskiye
        public ActionResult Index()
        {
            var values = c.SalesTransactions
                .Include(x => x.Product)
                .Include(x => x.Current)
                .Include(x => x.Personnel)
                .OrderByDescending(x => x.SalesTransactionDate)
                .ThenByDescending(x => x.SalesTransactionId)
                .ToList();
            return View(values);
        }

        // Satış formu
        public ActionResult SalesAdd(int? id)
        {
            FillLists(null);
            ViewBag.SelectedProductId = id ?? 0;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SalesAdd(SalesTransaction s)
        {
            var product = ValidateSale(s, null);
            if (!ModelState.IsValid)
            {
                FillLists(null);
                return View(s);
            }

            s.SalesTransactionDate = DateTime.Today;
            // Toplam formdan alınmaz, burada hesaplanır
            s.SalesTransactionTotalPrice = s.SalesTransactionQuantity * s.SalesTransactionUnitPrice;

            // STOK
            product.Stock = ToStock(product.Stock - s.SalesTransactionQuantity);

            c.SalesTransactions.Add(s);
            c.SaveChanges();

            TempData["Success"] = $"Satış kaydedildi. “{product.ProductName}” stoku {product.Stock} adede düştü.";
            return RedirectToAction("SalesDetail", new { id = s.SalesTransactionId });
        }

        // Güncelleme formu
        public ActionResult SalesGet(int id)
        {
            var value = c.SalesTransactions.Find(id);
            if (value == null)
            {
                return HttpNotFound();
            }

            FillLists(value);
            return View("SalesGet", value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SalesUpdate(SalesTransaction s)
        {
            // value
            var value = c.SalesTransactions.Find(s.SalesTransactionId);
            if (value == null)
            {
                return HttpNotFound();
            }

            var product = ValidateSale(s, value);

            if (!ReplaceBindingError("SalesTransactionDate", "Bir tarih seç.") && s.SalesTransactionDate.Year < 2000)
            {
                ModelState.AddModelError("SalesTransactionDate", "Tarih 2000 yılından önce olamaz.");
            }

            if (!ModelState.IsValid)
            {
                FillLists(value);
                return View("SalesGet", s);
            }

            // STOK düzeltmesi
            var oldProduct = c.Products.Find(value.ProductId);
            oldProduct.Stock = ToStock(oldProduct.Stock + value.SalesTransactionQuantity);
            product.Stock = ToStock(product.Stock - s.SalesTransactionQuantity);

            value.ProductId = s.ProductId;
            value.CurrentId = s.CurrentId;
            value.PersonnelId = s.PersonnelId;
            value.SalesTransactionQuantity = s.SalesTransactionQuantity;
            value.SalesTransactionUnitPrice = s.SalesTransactionUnitPrice;
            value.SalesTransactionTotalPrice = s.SalesTransactionQuantity * s.SalesTransactionUnitPrice;
            value.SalesTransactionDate = s.SalesTransactionDate;
            c.SaveChanges();

            TempData["Success"] = "Satış güncellendi.";
            return RedirectToAction("SalesDetail", new { id = value.SalesTransactionId });
        }

        // Satış fişi
        public ActionResult SalesDetail(int id)
        {
            var sale = c.SalesTransactions
                .Include(x => x.Product.Category)
                .Include(x => x.Current)
                .Include(x => x.Personnel.Department)
                .FirstOrDefault(x => x.SalesTransactionId == id);

            if (sale == null)
            {
                return HttpNotFound();
            }

            return View(sale);
        }

        // Formdaki listeler
        private void FillLists(SalesTransaction original)
        {
            int productId = original == null ? 0 : original.ProductId;
            int currentId = original == null ? 0 : original.CurrentId;

            ViewBag.Products = c.Products
                .Where(x => x.Status == true || x.ProductId == productId)
                .OrderBy(x => x.ProductName)
                .ToList();

            ViewBag.Currents = c.Currents
                .Where(x => x.Status == true || x.CurrentId == currentId)
                .OrderBy(x => x.CurrentName)
                .ThenBy(x => x.CurrentSurname)
                .ToList()
                .Select(x => new SelectListItem { Text = x.CurrentName + " " + x.CurrentSurname, Value = x.CurrentId.ToString() })
                .ToList();

            ViewBag.Personnels = c.Personnels
                .OrderBy(x => x.PersonnelName)
                .ThenBy(x => x.PersonnelSurname)
                .ToList()
                .Select(x => new SelectListItem { Text = x.PersonnelName + " " + x.PersonnelSurname, Value = x.PersonnelId.ToString() })
                .ToList();

            // Güncellemede bu satışın kendi adedi stok hesabına geri eklenir
            ViewBag.OriginalProductId = productId;
            ViewBag.OriginalQuantity = original == null ? 0 : original.SalesTransactionQuantity;
        }

        // Ekleme ve güncellemede ortak kontroller
        private Product ValidateSale(SalesTransaction s, SalesTransaction original)
        {
            Product product = null;
            if (!ReplaceBindingError("ProductId", "Bir ürün seç."))
            {
                product = c.Products.Find(s.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError("ProductId", "Bir ürün seç.");
                }
            }

            int currentId = s.CurrentId;
            if (!ReplaceBindingError("CurrentId", "Bir cari seç.") && !c.Currents.Any(x => x.CurrentId == currentId))
            {
                ModelState.AddModelError("CurrentId", "Bir cari seç.");
            }

            int personnelId = s.PersonnelId;
            if (!ReplaceBindingError("PersonnelId", "Bir personel seç.") && !c.Personnels.Any(x => x.PersonnelId == personnelId))
            {
                ModelState.AddModelError("PersonnelId", "Bir personel seç.");
            }

            bool quantityOk = !ReplaceBindingError("SalesTransactionQuantity", "Adet için tam sayı yaz.");
            if (quantityOk && s.SalesTransactionQuantity < 1)
            {
                ModelState.AddModelError("SalesTransactionQuantity", "Adet en az 1 olmalı.");
                quantityOk = false;
            }

            // Örnekteki ondalık ayracı sunucunun bölge ayarından gelir
            string priceFormat = "Fiyatı sayı olarak yaz. Örnek: " + (1250.5m).ToString("0.00");
            if (!ReplaceBindingError("SalesTransactionUnitPrice", priceFormat) && s.SalesTransactionUnitPrice <= 0)
            {
                ModelState.AddModelError("SalesTransactionUnitPrice", "Birim fiyat 0'dan büyük olmalı.");
            }

            if (product != null && quantityOk)
            {
                int available = product.Stock;
                if (original != null && original.ProductId == product.ProductId)
                {
                    available += original.SalesTransactionQuantity;
                }

                if (s.SalesTransactionQuantity > available)
                {
                    ModelState.AddModelError("SalesTransactionQuantity",
                        available == 0 ? "Bu ürün stokta yok." : $"Stokta yalnızca {available} adet var.");
                }
            }

            return product;
        }

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

        // Stock alanı short olduğu için sonucu 0 - 32767 aralığında tutar
        private static short ToStock(int value)
        {
            return (short)Math.Max(0, Math.Min(short.MaxValue, value));
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
