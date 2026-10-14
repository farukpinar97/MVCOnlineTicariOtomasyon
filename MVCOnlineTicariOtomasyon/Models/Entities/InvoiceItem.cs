using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCOnlineTicariOtomasyon.Models.Entities
{
    // Display ve hata mesajları veritabanı şemasını değiştirmez
    public class InvoiceItem
    {
        [Key]
        public int InvoiceItemId { get; set; }

        [Display(Name = "Açıklama")]
        [Column(TypeName = "varchar")]
        [StringLength(100, ErrorMessage = "Açıklama en fazla 100 karakter olabilir.")]
        public string InvoiceItemDescription { get; set; }

        [Display(Name = "Miktar")]
        public int InvoiceItemQuantity { get; set; }

        [Display(Name = "Birim fiyat")]
        public decimal InvoiceItemUnitPrice { get; set; }

        [Display(Name = "Tutar")]
        public decimal InvoiceItemTotalPrice { get; set; }

        public int InvoiceId { get; set; }
        public virtual Invoice Invoice { get; set; }
    }
}
