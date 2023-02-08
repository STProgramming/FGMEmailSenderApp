using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGMEmailSenderApp.Models.EntityFrameworkModels
{
    [Table("City")]
    public class City
    {
        [Key]
        [Required]
        public int IdCity { get; set; }

        [Required]
        public string NameCity { get; set; }

        [Required]
        public string CapCity { get; set; }

        [ForeignKey(nameof(Country))]
        public int FK_IdCountry { get; set; }

        public virtual Country Country { get; set; }

        public int FK_IdDepartment { get; set; }

    }
}
