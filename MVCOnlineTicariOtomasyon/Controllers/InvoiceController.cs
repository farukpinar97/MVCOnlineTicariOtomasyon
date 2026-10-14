using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.Entities;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    [AdminAuthorize]   // sadece yönetici girişi yapanlar
    public class InvoiceController : Controller
    {
        Context c = new Context();

        // Bütün faturalar, yeniden eskiye
        public ActionResult Index()
        {
            var list = c.Invoices
                .Include(x => x.InvoiceItems)
                .OrderByDescending(x => x.InvoiceDate)
                .ThenByDescending(x => x.InvoiceId)
                .ToList();
            return View(list);
        }

        // Ekleme formu
        public ActionResult InvoiceAdd()
        {
            var invoice = new Invoice
            {
                InvoiceDate = DateTime.Today,
                InvoiceTime = DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture)
            };
            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InvoiceAdd(Invoice f)
        {
            ValidateInvoice(f);
            if (!ModelState.IsValid)
            {
                return View(f);
            }

            f.Total = 0;   // toplam, kalemler eklendikçe hesaplanır
            c.Invoices.Add(f);
            c.SaveChanges();

            TempData["Success"] = "Fatura oluşturuldu. Şimdi kalemlerini ekleyebilirsin.";
            return RedirectToAction("InvoiceDetail", new { id = f.InvoiceId });
        }

        // Güncelleme formu
        public ActionResult InvoiceGet(int id)
        {
            var invoice = c.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }

            return View("InvoiceGet", invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InvoiceUpdate(Invoice f)
        {
            var invoice = c.Invoices.Find(f.InvoiceId);
            if (invoice == null)
            {
                return HttpNotFound();
            }

            ValidateInvoice(f);
            if (!ModelState.IsValid)
            {
                return View("InvoiceGet", f);
            }

            invoice.InvoiceSerialNo = f.InvoiceSerialNo;
            invoice.InvoiceOrderNo = f.InvoiceOrderNo;
            invoice.InvoiceDate = f.InvoiceDate;
            invoice.InvoiceTime = f.InvoiceTime;
            invoice.TaxOffice = f.TaxOffice;
            invoice.Deliverer = f.Deliverer;
            invoice.Receiver = f.Receiver;
            c.SaveChanges();

            TempData["Success"] = "Fatura güncellendi.";
            return RedirectToAction("InvoiceDetail", new { id = invoice.InvoiceId });
        }

        // Faturanın kendisi
        public ActionResult InvoiceDetail(int id)
        {
            var invoice = c.Invoices
                .Include(x => x.InvoiceItems)
                .FirstOrDefault(x => x.InvoiceId == id);

            if (invoice == null)
            {
                return HttpNotFound();
            }

            return View(invoice);
        }

        // Kalem ekleme formu
        public ActionResult InvoiceItemAdd(int id)
        {
            var invoice = c.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }

            ViewBag.Invoice = invoice;

            if (ControllerContext.IsChildAction || Request.IsAjaxRequest())
            {
                return PartialView("_InvoiceItemModal");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InvoiceItemAdd(InvoiceItem p)
        {
            var invoice = c.Invoices.Find(p.InvoiceId);
            if (invoice == null)
            {
                return HttpNotFound();
            }

            ValidateItem(p);
            if (!ModelState.IsValid)
            {
                ViewBag.Invoice = invoice;

                // Pencereden geldiyse sadece pencerenin içi döner, sayfa yenilenmez
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_InvoiceItemModal", p);
                }

                return View(p);
            }

            // Tutar formdan alınmaz, burada hesaplanır
            p.InvoiceItemTotalPrice = p.InvoiceItemQuantity * p.InvoiceItemUnitPrice;
            c.InvoiceItems.Add(p);
            c.SaveChanges();
            RecalculateTotal(invoice);

            TempData["Success"] = $"“{p.InvoiceItemDescription}” faturaya eklendi.";
            string target = Url.Action("InvoiceDetail", new { id = invoice.InvoiceId });

            if (Request.IsAjaxRequest())
            {
                // Pencereye bu işaret yerleşince modal-form.js fatura sayfasını yeniden açar
                return Content("<div data-redirect=\"" + HttpUtility.HtmlAttributeEncode(target) + "\"></div>", "text/html");
            }

            return Redirect(target);
        }

        // Kalem silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InvoiceItemDelete(int id)
        {
            var item = c.InvoiceItems.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            var invoice = c.Invoices.Find(item.InvoiceId);
            string description = item.InvoiceItemDescription;

            c.InvoiceItems.Remove(item);
            c.SaveChanges();
            RecalculateTotal(invoice);

            TempData["Success"] = $"“{description}” faturadan silindi.";
            return RedirectToAction("InvoiceDetail", new { id = invoice.InvoiceId });
        }

        // Faturanın toplamını kalemlerinden yeniden hesaplar ve kaydeder
        private void RecalculateTotal(Invoice invoice)
        {
            int invoiceId = invoice.InvoiceId;
            invoice.Total = c.InvoiceItems
                .Where(x => x.InvoiceId == invoiceId)
                .Sum(x => (decimal?)x.InvoiceItemTotalPrice) ?? 0;   // hiç kalem yoksa 0
            c.SaveChanges();
        }

        // Fatura üst bilgilerinin kontrolleri
        private void ValidateInvoice(Invoice f)
        {
            f.InvoiceSerialNo = Clean(f.InvoiceSerialNo);
            f.InvoiceOrderNo = Clean(f.InvoiceOrderNo);
            f.InvoiceTime = Clean(f.InvoiceTime);
            f.TaxOffice = Clean(f.TaxOffice);
            f.Deliverer = Clean(f.Deliverer);
            f.Receiver = Clean(f.Receiver);

            if (f.InvoiceSerialNo == null)
            {
                ModelState.AddModelError("InvoiceSerialNo", "Seri no boş bırakılamaz.");
            }
            else
            {
                f.InvoiceSerialNo = f.InvoiceSerialNo.ToUpperInvariant();
            }

            if (f.InvoiceOrderNo == null)
            {
                ModelState.AddModelError("InvoiceOrderNo", "Sıra no boş bırakılamaz.");
            }

            // Aynı seri ve sıra numarasıyla ikinci fatura olmasın
            if (f.InvoiceSerialNo != null && f.InvoiceOrderNo != null)
            {
                string serial = f.InvoiceSerialNo;
                string order = f.InvoiceOrderNo;
                int id = f.InvoiceId;

                if (c.Invoices.Any(x => x.InvoiceSerialNo == serial && x.InvoiceOrderNo == order && x.InvoiceId != id))
                {
                    ModelState.AddModelError("InvoiceOrderNo", $"{serial} {order} numaralı bir fatura zaten var.");
                }
            }

            if (!ReplaceBindingError("InvoiceDate", "Bir tarih seç.") && f.InvoiceDate.Year < 2000)
            {
                ModelState.AddModelError("InvoiceDate", "Tarih 2000 yılından önce olamaz.");
            }

            // Saat isteğe bağlı
            if (f.InvoiceTime != null && !Regex.IsMatch(f.InvoiceTime, @"^([01]\d|2[0-3]):[0-5]\d$"))
            {
                ModelState.AddModelError("InvoiceTime", "Saati 09:30 biçiminde yaz.");
            }
        }

        // Kalem kontrolleri
        private void ValidateItem(InvoiceItem p)
        {
            p.InvoiceItemDescription = Clean(p.InvoiceItemDescription);
            if (p.InvoiceItemDescription == null)
            {
                ModelState.AddModelError("InvoiceItemDescription", "Açıklama boş bırakılamaz.");
            }

            if (!ReplaceBindingError("InvoiceItemQuantity", "Miktar için tam sayı yaz.") && p.InvoiceItemQuantity < 1)
            {
                ModelState.AddModelError("InvoiceItemQuantity", "Miktar en az 1 olmalı.");
            }

            // Örnekteki ondalık ayracı sunucunun bölge ayarından gelir
            string priceFormat = "Fiyatı sayı olarak yaz. Örnek: " + (1250.5m).ToString("0.00");
            if (!ReplaceBindingError("InvoiceItemUnitPrice", priceFormat) && p.InvoiceItemUnitPrice <= 0)
            {
                ModelState.AddModelError("InvoiceItemUnitPrice", "Birim fiyat 0'dan büyük olmalı.");
            }
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
