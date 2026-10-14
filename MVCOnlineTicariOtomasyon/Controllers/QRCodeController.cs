using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using QRCoder;
using System.Linq;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    // QR kod
    [AdminAuthorize]   // sadece yönetici girişi yapanlar
    public class QRCodeController : Controller
    {
        Context c = new Context();

        // QR koda yazılabilecek en uzun metin
        private const int MaxLength = 100;

        // Sayfa
        public ActionResult Index(string code)
        {
            code = (code ?? "").Trim();

            if (code.Length > MaxLength)
            {
                ModelState.AddModelError("code", $"En fazla {MaxLength} karakter yazabilirsin.");
            }
            else if (code.Length > 0)
            {
                // Yazılan bir takip koduysa kargonun bilgileri de gösterilir
                string upper = code.ToUpperInvariant();
                var cargo = c.CargoDetails.FirstOrDefault(x => x.TrackingCode == upper);
                if (cargo != null)
                {
                    code = upper;
                }
                ViewBag.Cargo = cargo;
            }

            ViewBag.Code = code;

            // Son eklenen kargolar
            ViewBag.RecentCodes = c.CargoDetails
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.CargoDetailId)
                .Select(x => x.TrackingCode)
                .Take(6)
                .ToList();

            return View();
        }

        // QR kodun PNG resmi
        public ActionResult Image(string text, bool download = false)
        {
            text = (text ?? "").Trim();
            if (text.Length == 0 || text.Length > MaxLength)
            {
                return new HttpStatusCodeResult(400, "Geçersiz metin");
            }

            byte[] png = CreatePng(text);

            if (download)
            {
                // Dosya adında sadece harf ve rakam kalsın
                string safeName = new string(text.Where(char.IsLetterOrDigit).Take(40).ToArray());
                return File(png, "image/png", "qr-" + (safeName.Length > 0 ? safeName : "kod") + ".png");
            }

            return File(png, "image/png");
        }

        // QR kodu üretir, PNG olarak döndürür
        private static byte[] CreatePng(string text)
        {
            using (var generator = new QRCodeGenerator())
            using (QRCodeData data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
            {
                var qrCode = new PngByteQRCode(data);
                return qrCode.GetGraphic(20);
            }
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
