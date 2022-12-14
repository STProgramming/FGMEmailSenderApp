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

        [Required]
        public bool NewsSenderAggrement { get; set; }

        /// <summary>
        /// Relazione con company uno a uno
        /// </summary>
        public int IdCompany { get; set; }
        public virtual Company? Company { get; set; }

        /// <summary>
        /// Relazione con request uno a molte
        /// </summary>
        public virtual ICollection<Request>? Requests { get; set; }
    }
}
