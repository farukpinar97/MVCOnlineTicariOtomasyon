using MVCOnlineTicariOtomasyon.Models.Entities;
using System.Collections.Generic;

namespace MVCOnlineTicariOtomasyon.Models.ViewModels
{
    // Kargo sayfalarının modelleri

    // Kargo listesinin bir satırı
    public class CargoListItem
    {
        public CargoDetail Cargo { get; set; }
        public CargoTracking LastStatus { get; set; }   // hiç durum girilmediyse null
        public bool IsDelivered { get; set; }   // son durum "Teslim edildi" mi?
    }

    // Takip sayfası
    public class CargoTrackViewModel
    {
        public CargoDetail Cargo { get; set; }
        public List<CargoTracking> Statuses { get; set; }   // yeniden eskiye
        public CargoTracking NewStatus { get; set; }   // formun değerleri
        public bool IsDelivered { get; set; }
    }
}
