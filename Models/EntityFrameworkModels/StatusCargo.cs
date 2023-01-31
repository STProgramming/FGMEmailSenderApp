using MessagePack;
using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations.Schema;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace FGMEmailSenderApp.Models.EntityFrameworkModels
{
    public enum EStatusCargo
    {
        Scaricato,
        InTransito,
        NonCaricato,
        Caricando,
        Scaricando,
        Problema,
        InPartenza,
        InArrivo,
        Consegnato,
        NonConsegnato
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


        public virtual ICollection<CargoEvent> CargosEvent { get; set; }
    }
}
