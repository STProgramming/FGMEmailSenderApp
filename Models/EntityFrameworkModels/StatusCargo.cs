using MessagePack;
using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations.Schema;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace FGMEmailSenderApp.Models.EntityFrameworkModels
{
    public enum Status
    {
        Scaricato,
        In_transito,
        Non_Caricato,
        Caricando,
        Scaricando,
        Problema
    }

    [Table("StatusCargo")]
    public class StatusCargo
    {
        public StatusCargo() { }

        [Key]
        [Required]
        public int IdStatusCargo { get; set; }

        [Required]
        public string NameStatusCargo { get; set; }

        public virtual ICollection<StatusCargo> StatusCargoes { get; set; }
    }
}
