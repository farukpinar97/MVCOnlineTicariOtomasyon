using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCOnlineTicariOtomasyon.Models.Entities
{
    // Display ve hata mesajları veritabanı şemasını değiştirmez
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Display(Name = "Seri no")]
        [Column(TypeName = "char")]
        [StringLength(1, ErrorMessage = "Seri no tek karakter olmalı.")]
        public string InvoiceSerialNo { get; set; }

        [Display(Name = "Sıra no")]
        [Column(TypeName = "varchar")]
        [StringLength(6, ErrorMessage = "Sıra no en fazla 6 karakter olabilir.")]
        public string InvoiceOrderNo { get; set; }

        [Display(Name = "Tarih")]
        public DateTime InvoiceDate { get; set; }

        [Display(Name = "Vergi dairesi")]
        [Column(TypeName = "varchar")]
        [StringLength(60, ErrorMessage = "Vergi dairesi en fazla 60 karakter olabilir.")]
        public string TaxOffice { get; set; }

        [Display(Name = "Saat")]
        [Column(TypeName = "char")]
        [StringLength(5)]
        public string InvoiceTime { get; set; }

        [Display(Name = "Teslim eden")]
        [Column(TypeName = "varchar")]
        [StringLength(30, ErrorMessage = "Teslim eden en fazla 30 karakter olabilir.")]
        public string Deliverer { get; set; }

        [Display(Name = "Teslim alan")]
        [Column(TypeName = "varchar")]
        [StringLength(30, ErrorMessage = "Teslim alan en fazla 30 karakter olabilir.")]
        public string Receiver { get; set; }

        [Display(Name = "Toplam")]
        public decimal Total { get; set; }

        public ICollection<InvoiceItem> InvoiceItems { get; set; }
    }
}
