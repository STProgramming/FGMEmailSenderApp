namespace FGMEmailSenderApp.Models.ViewModels
{
    public class UserViewModel
    {
        public string Email { get; set; }

        public string Name { get; set; }

        public string LastName { get; set; }
        
        public bool NewsSenderAggrement { get; set; }

        public string UserName { get; set; }

        public string PhoneNumber { get; set; }

        public bool PhoneConfirmed { get; set; }

        public bool TwoFactoryEnabled { get; set; }

        public string? NameCompany  { get; set; }

        public string? IvaCompany { get; set; }
    }
}
