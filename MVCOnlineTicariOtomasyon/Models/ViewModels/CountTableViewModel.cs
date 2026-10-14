using System.Collections.Generic;

namespace MVCOnlineTicariOtomasyon.Models.ViewModels
{
    // Hızlı bakış sayfasındaki "neye göre kaç tane" tablolarının ortak modeli
    public class CountTableViewModel
    {
        public string Title { get; set; }   // kartın başlığı
        public string NameHeader { get; set; }   // ilk sütunun başlığı
        public string CountHeader { get; set; }   // sayı sütununun başlığı
        public string LinkUrl { get; set; }   // başlığın yanındaki link
        public string LinkText { get; set; }
        public int Total { get; set; }   // bütün satırların toplamı
        public List<CountRow> Rows { get; set; }
    }

    // Tablonun bir satırı
    public class CountRow
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public bool IsOther { get; set; }   // küçük satırların toplandığı "Diğer" satırı mı?
    }
}
