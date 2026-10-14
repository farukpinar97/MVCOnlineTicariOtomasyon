# 🧾 MVC Online Ticari Otomasyon

Bu proje, Udemy'deki **Murat Yücedağ** eğitmenliğindeki *MVC Online Ticari Otomasyon* kursu kapsamında geliştirilmiş bir ASP.NET MVC 5 uygulamasıdır. Ürün, stok, satış, fatura, personel ve cari takibini tek panelde toplar. Kursun temel yapısının üzerine özel bir arayüz teması, cari paneli, kargo modülü, grafikler ve rol bazlı yetkilendirme eklenmiştir. Tüm veriler MSSQL üzerinden dinamik olarak gelir.

## 📋 Proje Hakkında

Uygulama iki taraftan oluşur: yöneticinin işletmeyi yönettiği **yönetim paneli** ve müşterilerin (carilerin) kendi bilgilerini, siparişlerini, mesajlarını ve kargolarını takip ettiği **cari paneli**.

Arayüz kursun hazır temaları yerine, bir muhasebe defterinden esinlenen özel bir tasarımla baştan yazılmıştır: yeşil ve kehribar tonları, çizgili tablolar, "fiş" görünümlü fatura ve hata sayfaları. Tüm sayfalar masaüstünde tam genişlikte, mobilde tek sütunda düzgün çalışır.

## 🎯 Öne Çıkan Özellikler

### 🗂️ Yönetim Paneli

**İstatistikler** — Kasadaki toplam tutar, toplam satış, aktif ürün ve cari kartları; bugünkü kasa, son 7 günün satış grafiği, stok ve kategori özetleri

**Hızlı Bakış** — Kategori, şehir, departman ve markaya göre dağılım tabloları, oran çubuklarıyla

**Grafikler** — Chart.js ile dönem seçmeli (7 gün / 30 gün / 90 gün / 12 ay) ciro grafiği, önceki dönemle karşılaştırma, en çok satan ürünler, kategori payları ve personel performansı

**Ürünler ve Kategoriler** — CRUD işlemleri, sunucu taraflı arama ve sayfalama (PagedList), düşük stok uyarıları, ürün detay sayfası ve ürün galerisi

**Satışlar** — Satış kaydında stok otomatik düşer; liste DataTables ile sıralanır, filtrelenir, Excel / CSV olarak dışa aktarılır

**Faturalar** — Fiş görünümlü fatura detayı, fatura kalemlerinin sayfa yenilenmeden popup (modal) ile eklenmesi, yazdırma görünümü

**Personeller ve Departmanlar** — Fotoğraf yükleme ve güncelleme (dosya türü ve boyut kontrolüyle), personel bazlı satış geçmişi; silme onayı ve bildirimler SweetAlert2 ile

**Kargo Takibi** — Kargo eklerken 10 haneli benzersiz takip kodu üretilir, durum geçmişi zaman çizelgesi olarak tutulur, takip kodundan QR kod oluşturulur (QRCoder)

**Yapılacaklar** — Sayaç kartlarıyla birlikte basit görev listesi

### 👤 Cari Paneli

**Profil** — Sipariş ve harcama özeti, bilgileri güncelleme ve şifre değiştirme

**Siparişlerim** — Carinin kendi satın aldığı ürünler

**Mesajlar** — Cariler arası mesajlaşma: gelen kutusu, gönderilenler, okundu bilgisi, mesaj detayı ve yanıtlama

**Kargo Takibi** — Takip koduyla sorgulama, adım adım ilerleme çubuğu ve kargo hareketleri; cariye ait kargoların listesi

### 🔐 Güvenlik ve Yetkilendirme

**Kimlik Doğrulama** — Forms Authentication ile yönetici ve cari için ayrı giriş akışları

**Rol Bazlı Erişim** — Yöneticiler tam yetkili (A) ve sınırlı yetkili (B) olarak ayrılır; sınırlı yönetici cari, departman ve personel sayfalarını menüde görmez ve bu sayfalara giremez. Özel `AdminAuthorize` ve `CurrentAuthorize` filtreleriyle korunur

**Hata Sayfaları** — Tema ile uyumlu 404 (sayfa bulunamadı) ve 403 (yetkisiz erişim) sayfaları

**Form Güvenliği** — Tüm formlarda Anti-Forgery Token, Post/Redirect/Get akışı ve Türkçe doğrulama mesajları

## 🛠️ Kullanılan Teknolojiler

| Teknoloji | Açıklama |
|---|---|
| ASP.NET MVC 5 | .NET Framework 4.7.2, Razor view'lar |
| Entity Framework 6 | Code First yaklaşımı ve migration'lar |
| MSSQL Server | İlişkisel veritabanı |
| Forms Authentication | Çerez tabanlı giriş, özel yetkilendirme filtreleri |
| Bootstrap 4 + jQuery | Arayüz ve AJAX işlemleri |
| Chart.js | Grafikler sayfası |
| SweetAlert2 | Onay pencereleri ve bildirimler |
| DataTables | Satış listesinde sıralama, arama ve dışa aktarma |
| PagedList.Mvc | Sunucu taraflı sayfalama |
| QRCoder | Kargo takip kodundan QR kod üretimi |

## 📷 Ekran Görüntüleri

<img width="1920" height="945" alt="01" src="https://github.com/user-attachments/assets/5e889f48-5faa-4967-a647-af750208e34b" />

<img width="1920" height="1191" alt="02" src="https://github.com/user-attachments/assets/1a2e4d0e-9348-415d-9696-3687033efc91" />

<img width="1920" height="1516" alt="03" src="https://github.com/user-attachments/assets/0e5d0e01-67b7-454c-8022-f305d6542212" />

<img width="1920" height="1806" alt="04" src="https://github.com/user-attachments/assets/684a809e-2ab8-4b65-9fe2-97c710f958ca" />

<img width="1920" height="945" alt="05" src="https://github.com/user-attachments/assets/45a0930d-100e-4417-81dc-e34c54e6c5d1" />

<img width="1920" height="955" alt="06" src="https://github.com/user-attachments/assets/65ea4e3e-57aa-424a-814c-4cbb745d5777" />

<img width="1920" height="945" alt="07" src="https://github.com/user-attachments/assets/eb812f04-f834-4248-a421-5808167de643" />

<img width="1920" height="945" alt="08" src="https://github.com/user-attachments/assets/55552601-8ff2-46ae-bfb3-b153719e6686" />

<img width="1920" height="1622" alt="09" src="https://github.com/user-attachments/assets/38e1abe8-a4bd-4f5a-8c45-5c741b9223d6" />

<img width="1920" height="945" alt="10" src="https://github.com/user-attachments/assets/d59208e6-1b20-4a45-8eed-c42c70db3937" />

<img width="1920" height="945" alt="11" src="https://github.com/user-attachments/assets/8f16813d-b334-4a16-8441-87bfdaf98865" />

<img width="1920" height="945" alt="12" src="https://github.com/user-attachments/assets/15e151dd-564e-478f-84d5-bacba48c3bfa" />

<img width="1920" height="945" alt="13" src="https://github.com/user-attachments/assets/03cca3a3-d1ac-468e-b050-3277c78c2f6f" />

<img width="1920" height="945" alt="14" src="https://github.com/user-attachments/assets/ae744a16-9b81-43df-907e-b8643c1bd27e" />

<img width="1920" height="945" alt="15" src="https://github.com/user-attachments/assets/3577a1b1-a22c-40f7-81fe-fdf7ac08967d" />

<img width="1920" height="953" alt="16" src="https://github.com/user-attachments/assets/7e54b5c1-c056-4a91-9c86-68a307a1b657" />

<img width="1920" height="945" alt="17" src="https://github.com/user-attachments/assets/0d528726-a4d6-4173-8f41-61335c534631" />

<img width="1920" height="1018" alt="18" src="https://github.com/user-attachments/assets/44494eea-c27f-4bfc-b19f-63fd3608b593" />

<img width="1920" height="945" alt="19" src="https://github.com/user-attachments/assets/e37a3fd5-e705-4062-be2f-9e6133a5724f" />

<img width="1920" height="945" alt="20" src="https://github.com/user-attachments/assets/484f0d67-3b19-4dbe-b63f-53462a984d9d" />
