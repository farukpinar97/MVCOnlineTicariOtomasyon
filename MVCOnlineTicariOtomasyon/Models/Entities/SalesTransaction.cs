using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCOnlineTicariOtomasyon.Models.Entities
{
    // Display
    public class SalesTransaction
    {
        [Key]
        public int SalesTransactionId { get; set; }
        // ürün - müşteri - personel

        [Display(Name = "Tarih")]
        public DateTime SalesTransactionDate { get; set; }

        [Display(Name = "Adet")]
        public int SalesTransactionQuantity { get; set; }

        [Display(Name = "Birim fiyat")]
        public decimal SalesTransactionUnitPrice { get; set; }

        [Display(Name = "Toplam")]
        public decimal SalesTransactionTotalPrice { get; set; }

        [Display(Name = "Ürün")]
        public int ProductId { get; set; }

        [Display(Name = "Cari")]
        public int CurrentId { get; set; }

        [Display(Name = "Personel")]
        public int PersonnelId { get; set; }

        public virtual Product Product { get; set; }
        public virtual Current Current { get; set; }
        public virtual Personnel Personnel { get; set; }
    }
}
