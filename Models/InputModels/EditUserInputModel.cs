using Duende.IdentityServer.Models;
using System.ComponentModel.DataAnnotations;

namespace FGMEmailSenderApp.Models.InputModels
{
    public class EditUserInputModel
    {
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "In questo campo sono permesse solo l'inserimento delle lettere")]
        public string? NameUser { get; set; }

        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "In questo campo sono permesse solo l'inserimento delle lettere")]
        public string? LastNameUser { get; set; }

        public string? UserName { get; set; }

        public bool TwoFactAuth { get; set; }

        public bool NewsSenderAgreement { get; set; }
    }
}
