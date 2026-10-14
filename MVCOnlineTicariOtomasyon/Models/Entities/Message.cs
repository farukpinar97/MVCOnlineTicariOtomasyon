using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCOnlineTicariOtomasyon.Models.Entities
{
    // Cariler arasındaki mesaj
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Display(Name = "Konu")]
        [Column(TypeName = "varchar")]
        [StringLength(100)]
        public string Subject { get; set; }

        [Display(Name = "Mesaj")]
        [Column(TypeName = "varchar")]
        [StringLength(2000)]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        // Alıcı mesajı açınca true olur
        public bool IsRead { get; set; }

        [Display(Name = "Alıcı")]
        public int? ReceiverId { get; set; }
        [ForeignKey("ReceiverId")]
        public virtual Current Receiver { get; set; }

        public int? SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual Current Sender { get; set; }
    }
}
