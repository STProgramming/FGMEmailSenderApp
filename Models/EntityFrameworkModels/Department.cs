using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace FGMEmailSenderApp.Models.EntityFrameworkModels
{
    [Table("Department")]
    public class Department
    {
        public Department() { }

        [Key]
        [Required]
        public int IdDepartment { get; set; }

        [Required]
        public string NameDepartment { get; set; }

        [Required]
        public string CodeDepartment { get; set; }

        [ForeignKey(nameof(Country))]
        public int FK_IdCountry { get; set; }

        public virtual Country Country { get; set; }

        public virtual ICollection<City> Cities { get; set; }
    }
}
