using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGMEmailSenderApp.Models.EntityFrameworkModels
{
    [Table("Cargo")]
    public class Cargo
    {
        public Cargo() { }

        #region Properties of cargo

        [Key]
        [Required]
        public int IdCargo { get; set; }

        [Required]
        public string TitleCargo { get; set; }

        [Required]
        public string DetailCargo { get; set; }

        public string DescriptionCargo { get; set; }

        public string NoteCargo { get; set; }

        [Column(TypeName = "decimal(7,4)")]
        [Required]
        public decimal HeightCargo { get; set; }

        [Column(TypeName = "decimal(7,4)")]
        [Required]
        public decimal LenghtCargo { get; set; }

        [Column(TypeName = "decimal(7,4)")]
        [Required]
        public decimal DepthCargo { get; set; }

        [Column(TypeName = "decimal(7,4)")]
        [Required]
        public decimal WeightCargo { get; set; }

        #endregion

        #region Loading properties of cargo

        [Column(TypeName = "Date")]
        [Required]
        public DateTime LoadingDate { get; set; }

        [Required]
        public string LoadingAddress { get; set; }

        [Required]
        public int CapCityLoading { get; set; }

        [Required]
        [EmailAddress]
        public string CompanySenderCargoEmail { get; set; }

        [Required]
        public string CompanySenderCargoIva { get; set; }

        #endregion

        #region Delivery properties of cargo

        [Column(TypeName = "Date")]
        [Required]
        public DateTime DeliveryDate { get; set; }

        [Required]
        public string DeliveryAddress { get; set; }

        [Required]
        public int CapCityDelivery { get; set; }

        [Required]
        [EmailAddress]
        public string CompanyReceiverCargoEmail { get; set; }

        [Required]
        public string CompanyReceiverCargoIva { get; set; }

        #endregion

        #region Foreign Keys of cargo
        public int FK_IdDepartmentLoading { get; set; }

        public int FK_IdDepartmentDelivery { get; set; }

        /// <summary>
        /// Relazione uno a molti tra carichi e compagnie
        /// Qui abbiamo il lato dipendente molti
        /// </summary>
        [ForeignKey(nameof(Company))]
        public int FK_IdCompanySender { get; set; }

        public virtual Company CompanySender { get; set; }

        public int FK_IdCompanyReceiver { get; set; }

        public virtual ICollection<CargoEvent>? CargoEvents { get; set; }

        #endregion
    }
}
