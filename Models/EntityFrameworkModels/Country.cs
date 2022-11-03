using MessagePack;
using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations.Schema;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace FGMEmailSenderApp.Models.EntityFrameworkModels
{
    [Table("Country")]
    public class Country
    {
        public Country() { }

        [Key]
        [Required]
        public int IdCountry { get; set; }

        [Required]
        public string CountryName { get; set; }

        public virtual ICollection<Department> Departments { get; set; }
    }
}
