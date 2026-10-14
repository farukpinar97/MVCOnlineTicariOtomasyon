using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCOnlineTicariOtomasyon.Models.Entities
{
    // Display ve hata mesajları veritabanı şemasını değiştirmez
    public class Current
    {
        [Key]
        public int CurrentId { get; set; }

        [Display(Name = "Ad")]
        [Column(TypeName = "varchar")]
        [StringLength(30, ErrorMessage = "30 karakterden fazla veri girişi yapılamaz.")]
        public string CurrentName { get; set; }

        [Display(Name = "Soyad")]
        [Column(TypeName = "varchar")]
        [StringLength(30, ErrorMessage = "30 karakterden fazla veri girişi yapılamaz.")]
        [Required(ErrorMessage = "Bu alan boş geçilemez.")]
        public string CurrentSurname { get; set; }

        [Display(Name = "Şehir")]
        [Column(TypeName = "varchar")]
        [StringLength(13, ErrorMessage = "Şehir en fazla 13 karakter olabilir.")]
        public string CurrentCity { get; set; }

        [Display(Name = "E-posta")]
        [Column(TypeName = "varchar")]
        // E-posta biçimi burada [EmailAddress] ile DENETLENMEZ
        [StringLength(50, ErrorMessage = "E-posta en fazla 50 karakter olabilir.")]
        public string CurrentMail { get; set; }

        // [Required] BİLEREK YOK
        [Display(Name = "Şifre")]
        [Column(TypeName = "varchar")]
        [StringLength(20, ErrorMessage = "Şifre en fazla 20 karakter olabilir.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        public bool Status { get; set; }
        public ICollection<SalesTransaction> SalesTransactions { get; set; }
    }
}
