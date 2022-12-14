using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGMEmailSenderApp.Models.EntityFrameworkModels
{
    [Table("Request")]
    public class Request
    {
        [Key]
        [Required]
        public int IdRequest { get; set; }

        [Required]
        public string DescriptionRequest { get; set; }

        public bool? Response { get; set; }

        #region RELAZIONE CON TYPES RICHIESTE

        /// <summary>
        /// Relazione uno a molti con tipi di richieste
        /// </summary>

        [ForeignKey(nameof(TypeRequest))]
        public int IdTypesRequest { get; set; }
        public virtual TypeRequest TypesRequest { get; set; }

        #endregion

        #region RELAZIONE CON APPLICATION USER

        /// <summary>
        /// Relazione con application unser uno a molti
        /// lato request e' il molti
        /// </summary>

        [ForeignKey(nameof(ApplicationUser))]
        public string IdUser { get; set; }

        public virtual ApplicationUser User { get; set; }

        #endregion
    }
}
