using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGMEmailSenderApp.Models.EntityFrameworkModels
{
    public enum ETypeRequest
    {
        ModificaDatiAziendali,
        RichiestaAggiungaReferenteAziendale,
        PropostaCarico,
    }

    [Table("TypesRequest")]
    public class TypeRequest
    {
        [Required]
        [Key]
        public int IdTypeRequest{ get; set; }

        [Required]
        public string TypeNameRequest { get; set; }

        public virtual ICollection<Request> Requests { get; set; }
    }
}
