using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace FGMEmailSenderApp.Models.InputModels
{
    public class AddCompanyInputModel
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "In questo campo sono permesse solo l'inserimento delle lettere")]
        public string CompanyName { get; set; }

        [Required]
        [EmailAddress]
        public string CompanyEmail { get; set; }

        [Required]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Questo campo deve contenere solo numeri")]
        public string CompanyTel { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "Questo campo deve contenere solo numeri")]
        public string? CompanyFax { get; set; }

        [Required]
        public string CompanyIva { get; set; }
    }
}
