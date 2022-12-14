using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGMEmailSenderApp.Models.EntityFrameworkModels
{
    [Table("CargoEvent")]
    public class CargoEvent
    {
        [Key]
        public int Id { get; set; }

        public string NoteEvent { get; set; }

        [Required]
        public DateTime DateEvent { get; set; }

        [ForeignKey(nameof(StatusCargo))]
        public int FK_IdStatusCargo { get; set; }

        public virtual StatusCargo StatusCargoes { get; set; }

        [ForeignKey(nameof(Cargo))]
        public int FK_IdCargo { get; set; }

        public virtual Cargo Cargoes { get; set; }
    }
}
