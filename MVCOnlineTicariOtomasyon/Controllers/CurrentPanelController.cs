using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.Entities;
using MVCOnlineTicariOtomasyon.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    // Cari paneli
    [CurrentAuthorize]
    public class CurrentPanelController : Controller
    {
        Context c = new Context();

        // Giriş yapmış cari
        Current me;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Çerezdeki ad, carinin numarası
            int id;
            if (int.TryParse(User.Identity.Name, out id))
            {
                me = c.Currents.FirstOrDefault(x => x.CurrentId == id && x.Status == true);
            }

            // Cari bulunamadıysa
            if (me == null)
            {
                FormsAuthentication.SignOut();
                filterContext.Result = RedirectToAction("Index", "Login");
                return;
            }

            // Yan menüdeki kimlik kutusu
            ViewBag.CurrentFullName = me.CurrentName + " " + me.CurrentSurname;
            ViewBag.CurrentMail = me.CurrentMail;

            // Yan menüdeki "Mesajlar"ın yanında okunmamış mesaj sayısı
            int myId = me.CurrentId;
            ViewBag.UnreadCount = c.Messages.Count(x => x.ReceiverId == myId && x.IsRead == false);

            base.OnActionExecuting(filterContext);
        }

        // Profilim

        public ActionResult Index()
        {
            FillProfile();
            return View(me);
        }

        // Ad, soyad ve şehir güncellenir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileUpdate(Current p)
        {
            p.CurrentName = Clean(p.CurrentName);
            p.CurrentSurname = Clean(p.CurrentSurname);
            p.CurrentCity = Clean(p.CurrentCity);

            if (p.CurrentName == null)
            {
                ModelState.AddModelError("CurrentName", "Adını yaz.");
            }

            // Formda olmayan alanların kuralları burada denetlenmez
            foreach (var key in new[] { "CurrentMail", "CurrentPassword", "Status" })
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                p.CurrentId = me.CurrentId;
                p.CurrentMail = me.CurrentMail;
                return ProfileWithErrors(p);
            }

            me.CurrentName = p.CurrentName;
            me.CurrentSurname = p.CurrentSurname;
            me.CurrentCity = p.CurrentCity;
            c.SaveChanges();

            TempData["Success"] = "Bilgilerin güncellendi.";
            return RedirectToAction("Index");
        }

        // Şifre değiştirme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PasswordUpdate(string oldPassword, string newPassword, string newPasswordRepeat)
        {
            if (string.IsNullOrEmpty(oldPassword))
            {
                ModelState.AddModelError("OldPassword", "Şu anki şifreni yaz.");
            }
            else if (me.CurrentPassword != oldPassword)
            {
                ModelState.AddModelError("OldPassword", "Şu anki şifren hatalı.");
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError("NewPassword", "Yeni şifreni yaz.");
            }
            else if (newPassword.Length < 6 || newPassword.Length > 20)
            {
                ModelState.AddModelError("NewPassword", "Yeni şifre 6 ile 20 karakter arası olmalı.");
            }
            else if (newPassword == oldPassword)
            {
                ModelState.AddModelError("NewPassword", "Yeni şifre şu ankiyle aynı olamaz.");
            }
            else if (newPassword != newPasswordRepeat)
            {
                ModelState.AddModelError("NewPasswordRepeat", "Şifreler aynı değil.");
            }

            if (!ModelState.IsValid)
            {
                return ProfileWithErrors(me);
            }

            me.CurrentPassword = newPassword;
            c.SaveChanges();

            TempData["Success"] = "Şifren değiştirildi. Bir sonraki girişte yeni şifreni kullan.";
            return RedirectToAction("Index");
        }

        // Siparişlerim

        // Bu cariye yapılan satışlar, yeniden eskiye
        public ActionResult Orders()
        {
            int id = me.CurrentId;
            var values = c.SalesTransactions
                .Include(x => x.Product)
                .Where(x => x.CurrentId == id)
                .OrderByDescending(x => x.SalesTransactionDate)
                .ThenByDescending(x => x.SalesTransactionId)
                .ToList();

            return View(values);
        }

        // Kargo takibi

        // Kargonun yolculuğundaki adımlar
        public static readonly string[] CargoSteps = { "Kargo alındı", "Transfer merkezinde", "Dağıtıma çıktı", "Teslim edildi" };

        // Kargo takibi
        public ActionResult Cargo(string code)
        {
            code = Clean(code);
            if (code != null)
            {
                code = code.ToUpperInvariant();
            }

            CargoTrackViewModel model = null;
            if (code != null)
            {
                var cargo = c.CargoDetails.FirstOrDefault(x => x.TrackingCode == code);
                if (cargo == null)
                {
                    ModelState.AddModelError("code", $"“{code}” takip kodlu bir kargo bulunamadı. Kodu kontrol edip tekrar dene.");
                }
                else
                {
                    string trackingCode = cargo.TrackingCode;
                    var statuses = c.CargoTrackings
                        .Where(x => x.TrackingCode == trackingCode)
                        .OrderByDescending(x => x.Date)
                        .ThenByDescending(x => x.CargoTrackingId)
                        .ToList();

                    model = new CargoTrackViewModel
                    {
                        Cargo = cargo,
                        Statuses = statuses,
                        IsDelivered = statuses.Count > 0 && CargoController.IsDeliveredText(statuses[0].Description)
                    };
                    ViewBag.Stage = CargoStage(statuses);
                }
            }

            ViewBag.Code = code;
            ViewBag.MyCargoes = MyCargoes();
            ViewBag.PageTitle = "Kargo takibi";
            return View(model);
        }

        // Kargonun hangi adıma geldiği
        private static int CargoStage(List<CargoTracking> statuses)
        {
            int stage = 0;
            foreach (var s in statuses)
            {
                for (int i = 0; i < CargoSteps.Length; i++)
                {
                    if (s.Description != null && s.Description.IndexOf(CargoSteps[i], StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        stage = Math.Max(stage, i + 1);
                    }
                }
            }
            return stage;
        }

        // Alıcı adı bu carinin "Ad Soyad"ı olan kargolar
        private List<CargoListItem> MyCargoes()
        {
            string fullName = me.CurrentName + " " + me.CurrentSurname;
            var cargoes = c.CargoDetails
                .Where(x => x.Receiver == fullName)
                .OrderByDescending(x => x.Date)
                .Take(10)
                .ToList();

            var codes = cargoes.Select(x => x.TrackingCode).ToList();
            var lastStatuses = c.CargoTrackings
                .Where(x => codes.Contains(x.TrackingCode))
                .GroupBy(x => x.TrackingCode)
                .Select(g => g.OrderByDescending(x => x.Date).ThenByDescending(x => x.CargoTrackingId).FirstOrDefault())
                .ToList();

            return cargoes.Select(x =>
            {
                var last = lastStatuses.FirstOrDefault(t => string.Equals(t.TrackingCode, x.TrackingCode, StringComparison.OrdinalIgnoreCase));
                return new CargoListItem
                {
                    Cargo = x,
                    LastStatus = last,
                    IsDelivered = last != null && CargoController.IsDeliveredText(last.Description)
                };
            }).ToList();
        }

        // Mesajlar

        // Mesaj kutusu
        public ActionResult Messages(string folder, string q)
        {
            int id = me.CurrentId;
            bool sent = folder == "sent";
            q = Clean(q);

            var query = c.Messages.Include(x => x.Sender).Include(x => x.Receiver);
            query = sent ? query.Where(x => x.SenderId == id) : query.Where(x => x.ReceiverId == id);

            if (q != null)
            {
                // Gelen kutusunda gönderenin, gönderilenlerde alıcının adında da aranır
                query = query.Where(x => x.Subject.Contains(q)
                    || x.Content.Contains(q)
                    || (sent
                        ? (x.Receiver.CurrentName + " " + x.Receiver.CurrentSurname).Contains(q)
                        : (x.Sender.CurrentName + " " + x.Sender.CurrentSurname).Contains(q)));
            }

            var list = query
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.MessageId)
                .ToList();

            ViewBag.Folder = sent ? "sent" : "inbox";
            ViewBag.Query = q;
            ViewBag.PageTitle = "Mesajlar";
            FillMailCounts();
            return View(list);
        }

        // Mesajın kendisi
        public ActionResult MessageDetail(int id)
        {
            int myId = me.CurrentId;
            var message = c.Messages
                .Include(x => x.Sender)
                .Include(x => x.Receiver)
                .FirstOrDefault(x => x.MessageId == id && (x.SenderId == myId || x.ReceiverId == myId));

            if (message == null)
            {
                TempData["Error"] = "Mesaj bulunamadı.";
                return RedirectToAction("Messages");
            }

            if (message.ReceiverId == myId && !message.IsRead)
            {
                message.IsRead = true;
                c.SaveChanges();
                ViewBag.UnreadCount = c.Messages.Count(x => x.ReceiverId == myId && x.IsRead == false);
            }

            ViewBag.Incoming = message.ReceiverId == myId;   // gelen mi, gönderilen mi
            ViewBag.Folder = message.ReceiverId == myId ? "inbox" : "sent";
            FillMailCounts();
            ViewBag.ActiveTab = "Messages";
            ViewBag.PageTitle = "Mesaj";
            return View(message);
        }

        // Yeni mesaj formu
        public ActionResult NewMessage(int? to, int? reply)
        {
            var model = new Message { ReceiverId = to };

            var original = FindReplyTo(reply);
            if (original != null)
            {
                // Gelen mesaja yanıt gönderene, kendi gönderdiğine "yanıt" yine aynı alıcıya gider
                int myId = me.CurrentId;
                model.ReceiverId = original.ReceiverId == myId ? original.SenderId : original.ReceiverId;

                string subject = original.Subject ?? "";
                model.Subject = subject.StartsWith("Ynt:", StringComparison.OrdinalIgnoreCase) ? subject : "Ynt: " + subject;
                if (model.Subject.Length > 100)
                {
                    model.Subject = model.Subject.Substring(0, 100);
                }
            }

            return ComposeView(model, original);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewMessage(Message m, int? reply)
        {
            int myId = me.CurrentId;
            m.Subject = Clean(m.Subject);
            m.Content = string.IsNullOrWhiteSpace(m.Content) ? null : m.Content.Trim();

            // Sınıftaki StringLength'in İngilizce mesajlarını Türkçesiyle değiştir
            ReplaceErrors("Subject", "Konu en fazla 100 karakter olabilir.");
            ReplaceErrors("Content", "Mesaj en fazla 2000 karakter olabilir.");

            int receiverId = m.ReceiverId ?? 0;
            if (receiverId == myId)
            {
                ModelState.AddModelError("ReceiverId", "Kendine mesaj gönderemezsin.");
            }
            else if (!c.Currents.Any(x => x.CurrentId == receiverId && x.Status == true))
            {
                ModelState.AddModelError("ReceiverId", "Mesajın gideceği cariyi seç.");
            }

            if (m.Subject == null)
            {
                ModelState.AddModelError("Subject", "Bir konu yaz.");
            }

            if (m.Content == null)
            {
                ModelState.AddModelError("Content", "Mesajını yaz.");
            }

            if (!ModelState.IsValid)
            {
                return ComposeView(m, FindReplyTo(reply));
            }

            // Formdan sadece alıcı, konu ve mesaj alınır
            c.Messages.Add(new Message
            {
                SenderId = myId,
                ReceiverId = receiverId,
                Subject = m.Subject,
                Content = m.Content,
                Date = DateTime.Now,
                IsRead = false
            });
            c.SaveChanges();

            TempData["Success"] = "Mesajın gönderildi.";
            return RedirectToAction("Messages", new { folder = "sent" });
        }

        // Mesaj sayfalarının solundaki klasörlerin sayıları
        private void FillMailCounts()
        {
            int myId = me.CurrentId;
            ViewBag.InboxCount = c.Messages.Count(x => x.ReceiverId == myId);
            ViewBag.SentCount = c.Messages.Count(x => x.SenderId == myId);
        }

        // Yanıtlanan mesaj
        private Message FindReplyTo(int? reply)
        {
            if (!reply.HasValue)
            {
                return null;
            }

            int myId = me.CurrentId;
            return c.Messages
                .Include(x => x.Sender)
                .FirstOrDefault(x => x.MessageId == reply.Value && (x.SenderId == myId || x.ReceiverId == myId));
        }

        private ActionResult ComposeView(Message model, Message replyTo)
        {
            int myId = me.CurrentId;
            ViewBag.ReplyTo = replyTo;

            // Alıcı listesi
            ViewBag.Receivers = c.Currents
                .Where(x => x.Status == true && x.CurrentId != myId)
                .OrderBy(x => x.CurrentName).ThenBy(x => x.CurrentSurname)
                .ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.CurrentId.ToString(),
                    Text = x.CurrentName + " " + x.CurrentSurname + (string.IsNullOrEmpty(x.CurrentMail) ? "" : " (" + x.CurrentMail + ")"),
                    Selected = x.CurrentId == model.ReceiverId
                })
                .ToList();

            ViewBag.ActiveTab = "Messages";
            ViewBag.PageTitle = "Yeni mesaj";
            ViewBag.Folder = "new";
            FillMailCounts();
            return View("NewMessage", model);
        }

        // Yardımcılar

        // Profil sayfasının rakamları ve son üç sipariş
        private void FillProfile()
        {
            int id = me.CurrentId;
            var sales = c.SalesTransactions.Where(x => x.CurrentId == id);

            ViewBag.OrderCount = sales.Count();
            ViewBag.TotalSpent = sales.Sum(x => (decimal?)x.SalesTransactionTotalPrice) ?? 0;
            ViewBag.RecentOrders = sales
                .Include(x => x.Product)
                .OrderByDescending(x => x.SalesTransactionDate)
                .ThenByDescending(x => x.SalesTransactionId)
                .Take(3)
                .ToList();
        }

        // Formlardan biri hatalıysa profil sayfası hata mesajlarıyla yeniden gösterilir
        private ActionResult ProfileWithErrors(Current model)
        {
            FillProfile();
            ViewBag.ActiveTab = "Index";   // yan menüde "Profilim" seçili kalsın
            return View("Index", model);
        }

        // Bir alanın MVC'nin eklediği hatalarını bizim mesajımızla değiştirir
        private void ReplaceErrors(string key, string message)
        {
            if (!ModelState.IsValidField(key))
            {
                ModelState[key].Errors.Clear();
                ModelState.AddModelError(key, message);
            }
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
