using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.Entities;
using MVCOnlineTicariOtomasyon.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    // Kargo modülü
    [AdminAuthorize]   // sadece yönetici girişi yapanlar
    public class CargoController : Controller
    {
        Context c = new Context();

        // Takip kodu
        private const string CodeAlphabet = "ABCDEFGHJKLMNPRSTUVYZ23456789";
        private const int CodeLength = 10;

        // "Durum ekle" formundaki hazır seçenekler
        public static readonly string[] StatusOptions =
        {
            "Kargo alındı",
            "Transfer merkezinde",
            "Dağıtıma çıktı",
            "Teslim edildi",
            "Alıcı adreste bulunamadı",
            "Şubede bekliyor"
        };

        // Kargo kaydedilince otomatik eklenen ilk durum
        private const string FirstStatus = "Gönderi kaydı oluşturuldu";

        // Liste

        // Bütün kargolar, yeniden eskiye
        public ActionResult Index()
        {
            var cargoes = c.CargoDetails
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.CargoDetailId)
                .ToList();

            // Her takip kodunun en son durumu
            var lastStatuses = c.CargoTrackings
                .GroupBy(x => x.TrackingCode)
                .Select(g => g.OrderByDescending(x => x.Date).ThenByDescending(x => x.CargoTrackingId).FirstOrDefault())
                .ToList()
                .ToDictionary(x => x.TrackingCode, StringComparer.OrdinalIgnoreCase);

            var model = cargoes.Select(x =>
            {
                CargoTracking last;
                lastStatuses.TryGetValue(x.TrackingCode ?? "", out last);
                return new CargoListItem
                {
                    Cargo = x,
                    LastStatus = last,
                    IsDelivered = last != null && IsDeliveredText(last.Description)
                };
            }).ToList();

            return View(model);
        }

        // Ekleme

        // Ekleme formu
        public ActionResult CargoAdd()
        {
            FillLists();
            var cargo = new CargoDetail
            {
                TrackingCode = NewTrackingCode(),
                Date = TrimSeconds(DateTime.Now)
            };
            return View(cargo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CargoAdd(CargoDetail k)
        {
            string code = (k.TrackingCode ?? "").Trim().ToUpperInvariant();
            if (!IsValidCode(code) || c.CargoDetails.Any(x => x.TrackingCode == code))
            {
                code = NewTrackingCode();
            }
            k.TrackingCode = code;
            ModelState.Remove("TrackingCode");

            ValidateCargo(k);
            if (!ModelState.IsValid)
            {
                FillLists();
                return View(k);
            }

            c.CargoDetails.Add(k);

            // Durum geçmişi boş başlamasın
            c.CargoTrackings.Add(new CargoTracking
            {
                TrackingCode = k.TrackingCode,
                Description = FirstStatus,
                Date = k.Date
            });

            c.SaveChanges();

            TempData["Success"] = $"Kargo kaydedildi. Takip kodu: {k.TrackingCode}";
            return RedirectToAction("CargoTrack", new { id = k.TrackingCode });
        }

        // Takip

        // Takip kodlu kargonun sayfası
        public ActionResult CargoTrack(string id)
        {
            string code = (id ?? "").Trim().ToUpperInvariant();
            var cargo = code == "" ? null : c.CargoDetails.FirstOrDefault(x => x.TrackingCode == code);

            if (cargo == null)
            {
                TempData["Error"] = code == ""
                    ? "Aramak için bir takip kodu yaz."
                    : $"“{code}” takip kodlu bir kargo bulunamadı.";
                return RedirectToAction("Index");
            }

            var newStatus = new CargoTracking { TrackingCode = cargo.TrackingCode, Date = TrimSeconds(DateTime.Now) };
            return View(TrackModel(cargo, newStatus));
        }

        // Kargoya yeni durum ekler
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CargoStatusAdd(CargoTracking t)
        {
            string code = (t.TrackingCode ?? "").Trim().ToUpperInvariant();
            var cargo = code == "" ? null : c.CargoDetails.FirstOrDefault(x => x.TrackingCode == code);
            if (cargo == null)
            {
                return HttpNotFound();
            }

            t.TrackingCode = cargo.TrackingCode;
            t.Description = Clean(t.Description);

            ReplaceErrors("Description", "Durum en fazla 100 karakter olabilir.");
            if (t.Description == null)
            {
                ModelState.AddModelError("Description", "Durumu yaz ya da listeden seç.");
            }

            if (!ReplaceErrors("Date", "Bir tarih ve saat seç.") && t.Date < cargo.Date)
            {
                ModelState.AddModelError("Date", "Durumun tarihi kargonun gönderim tarihinden önce olamaz.");
            }

            if (!ModelState.IsValid)
            {
                return View("CargoTrack", TrackModel(cargo, t));
            }

            c.CargoTrackings.Add(t);
            c.SaveChanges();

            TempData["Success"] = $"Durum eklendi: {t.Description}";
            return RedirectToAction("CargoTrack", new { id = cargo.TrackingCode });
        }

        // Yanlış girilen durumu siler
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CargoStatusDelete(int id)
        {
            var status = c.CargoTrackings.Find(id);
            if (status == null)
            {
                return HttpNotFound();
            }

            string code = status.TrackingCode;
            c.CargoTrackings.Remove(status);
            c.SaveChanges();

            TempData["Success"] = $"“{status.Description}” durumu silindi.";
            return RedirectToAction("CargoTrack", new { id = code });
        }

        // Yardımcılar

        private CargoTrackViewModel TrackModel(CargoDetail cargo, CargoTracking newStatus)
        {
            string code = cargo.TrackingCode;
            var statuses = c.CargoTrackings
                .Where(x => x.TrackingCode == code)
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.CargoTrackingId)
                .ToList();

            ViewBag.StatusOptions = StatusOptions;

            return new CargoTrackViewModel
            {
                Cargo = cargo,
                Statuses = statuses,
                NewStatus = newStatus,
                IsDelivered = statuses.Count > 0 && IsDeliveredText(statuses[0].Description)
            };
        }

        // Rastgele 10 karakterlik takip kodu
        private string NewTrackingCode()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                while (true)
                {
                    var code = new StringBuilder(CodeLength);
                    var buffer = new byte[1];
                    int limit = 256 - (256 % CodeAlphabet.Length);   // eşit dağılım için üstteki değerler atlanır

                    while (code.Length < CodeLength)
                    {
                        rng.GetBytes(buffer);
                        if (buffer[0] < limit)
                        {
                            code.Append(CodeAlphabet[buffer[0] % CodeAlphabet.Length]);
                        }
                    }

                    string result = code.ToString();
                    if (!c.CargoDetails.Any(x => x.TrackingCode == result))
                    {
                        return result;
                    }
                }
            }
        }

        private static bool IsValidCode(string code)
        {
            return code.Length == CodeLength && code.All(ch => CodeAlphabet.IndexOf(ch) >= 0);
        }

        // Son durum "Teslim edildi" ise kargo teslim edilmiş sayılır
        public static bool IsDeliveredText(string description)
        {
            return description != null
                && description.IndexOf("teslim edildi", StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        // Kargo formunun kontrolleri
        private void ValidateCargo(CargoDetail k)
        {
            k.Receiver = Clean(k.Receiver);
            k.Personnel = Clean(k.Personnel);
            k.Description = Clean(k.Description);

            ReplaceErrors("Receiver", "Alıcı en fazla 25 karakter olabilir.");
            ReplaceErrors("Personnel", "Personel adı en fazla 25 karakter olabilir.");
            ReplaceErrors("Description", "Açıklama en fazla 300 karakter olabilir.");

            if (k.Receiver == null)
            {
                ModelState.AddModelError("Receiver", "Alıcının adını yaz.");
            }

            if (k.Personnel == null)
            {
                ModelState.AddModelError("Personnel", "Kargoyu gönderen personeli seç.");
            }

            if (!ReplaceErrors("Date", "Bir tarih ve saat seç.") && k.Date.Year < 2000)
            {
                ModelState.AddModelError("Date", "Tarih 2000 yılından önce olamaz.");
            }
        }

        // Formdaki listeler
        private void FillLists()
        {
            ViewBag.Personnels = c.Personnels
                .OrderBy(x => x.PersonnelName).ThenBy(x => x.PersonnelSurname)
                .ToList()
                .Select(x => x.PersonnelName + " " + x.PersonnelSurname)
                .Where(x => x.Length <= 25)   // kolon 25 karakter
                .Distinct()
                .ToList();

            ViewBag.Receivers = c.Currents
                .Where(x => x.Status == true)
                .OrderBy(x => x.CurrentName).ThenBy(x => x.CurrentSurname)
                .ToList()
                .Select(x => x.CurrentName + " " + x.CurrentSurname)
                .Where(x => x.Length <= 25)
                .Distinct()
                .ToList();
        }

        // Bir alanın MVC'nin eklediği hatalarını bizim mesajımızla değiştirir
        private bool ReplaceErrors(string key, string message)
        {
            if (ModelState.IsValidField(key))
            {
                return false;
            }

            ModelState[key].Errors.Clear();
            ModelState.AddModelError(key, message);
            return true;
        }

        // Saniyeleri at
        private static DateTime TrimSeconds(DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
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
