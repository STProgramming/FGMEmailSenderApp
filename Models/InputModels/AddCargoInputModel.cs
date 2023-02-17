using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGMEmailSenderApp.Models.InputModels
{
    public class AddCargoInputModel
    {
        [Required]
        public string TitleCargo { get; set; }

        [Required]
        public string DetailCargo { get; set; }

        public string DescriptionCargo { get; set; }

        public string NoteCargo { get; set; }

        [Required]        
        public decimal HeightCargo { get; set; }

        [Required]
        public decimal WeigthCargo { get; set; }

        [Required]
        public decimal LenghtCargo { get; set; }

        [Required]
        public decimal DepthCargo { get; set; }

        public DateTime? LoadingDate { get; set; }

        [Required]
        public string LoadingAddress { get; set; }

        [Required]
        public string CapCityLoading { get; set; }

        [Required]
        [EmailAddress]
        public string CompanySenderCargoEmail { get; set; }

        [Required]
        public string CompanySenderCargoIva { get; set; }


        public DateTime? DeliveryDate { get; set; }

        [Required]
        public string DeliveryAddress { get; set; }

        [Required]
        public string CapCityDelivery { get; set; }

        [Required]
        [EmailAddress]
        public string CompanyReceiverCargoEmail { get; set; }

        [Required]
        public string CompanyReceiverCargoIva { get; set; }

        [Required]
        public int StatusNumber { get; set; }

        public string? NoteCargoEvent { get; set; }
    }
}
