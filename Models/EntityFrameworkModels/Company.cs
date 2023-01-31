using MessagePack;
using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace FGMEmailSenderApp.Models.EntityFrameworkModels
{
    [Table("Company")]
    public class Company
    {
        public Company() { }

        #region Properties Company

        [Key]
        [Required]
        public int IdCompany { get; set; }

        [Required]
        //TODO in a second code wave need to provide the data annotation to passing a correct form for all properties
        public string CompanyName { get; set; }

        [Required]
        [EmailAddress]
        public string CompanyEmail { get; set; }

        [Required]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Questo campo deve contenere solo numeri")]
        public string CompanyTel { get; set; }

        [Required]
        [MinLength(11)]
        [MaxLength(11)]
        //TODO create here a custom regex in regular expression data annotation that can contains numbers and letters only
        public string CompanyIva { get; set; }

        public string? CompanyFax { get; set; }

        #endregion

        #region Foreign Keys

        public string IdUser { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Cargo> Cargos { get; set; }
        #endregion
    }
}
