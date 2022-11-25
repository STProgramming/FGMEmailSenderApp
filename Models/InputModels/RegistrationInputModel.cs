using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace FGMEmailSenderApp.Models.InputModels
{
    public class RegistrationInputModel
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "In questo campo sono permesse solo l'inserimento delle lettere")]
        public string NameUser { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "In questo campo sono permesse solo l'inserimento delle lettere")]
        public string LastNameUser { get; set; }

        [Required]
        [EmailAddress]
        public string EmailUser { get; set; }

        [Required]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Questo campo deve contenere solo numeri")]
        [MinLength(9, ErrorMessage = "Il numero di telefono inserito non e' corretto")]
        [MaxLength(11, ErrorMessage = "Il numero di telefono inserito non e' corretto")]
        public string PhoneUser { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public bool NewsSenderAggrement { get; set; }
    }
}
