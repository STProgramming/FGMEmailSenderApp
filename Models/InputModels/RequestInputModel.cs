using System.ComponentModel.DataAnnotations;

namespace FGMEmailSenderApp.Models.InputModels
{
    public class RequestInputModel
    {
        [Required]
        public string DescriptionRequest { get; set; }

        [Required]
        public int IdTypeRequest { get; set; }
    }
}
