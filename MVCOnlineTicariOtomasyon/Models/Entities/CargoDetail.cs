using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCOnlineTicariOtomasyon.Models.Entities
{
    public class CargoDetail
    {
        [Key]
        public int CargoDetailId { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(300)]
        public string Description { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(10)]
        public string TrackingCode { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(25)]
        public string Personnel { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(25)]
        public string Receiver { get; set; }
        public DateTime Date { get; set; }
    }
}