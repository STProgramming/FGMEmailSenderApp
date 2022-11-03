using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FGMEmailSenderApp.Models.EntityFrameworkModels
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "In questo campo sono permesse solo l'inserimento delle lettere")]
        public string NameUser { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "In questo campo sono permesse solo l'inserimento delle lettere")]
        public string LastNameUser { get; set; }

        public virtual Company Company { get; set; }

        [Required]
        public bool NewsSenderAggrement { get; set; }
    }
}
