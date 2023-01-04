using System.ComponentModel.DataAnnotations;

namespace FGMEmailSenderApp.Models.InputModels
{
    public class EditCompanyInputModel
    {
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "In questo campo sono permesse solo l'inserimento delle lettere")]
        public string CompanyName { get; set; }

        [EmailAddress] 
        public string CompanyEmail { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "Questo campo deve contenere solo numeri")]
        public string CompanyTel { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "Questo campo deve contenere solo numeri")]
        public string CompanyFax { get; set; }

        public string CompanyIva { get; set; }
    }
}
