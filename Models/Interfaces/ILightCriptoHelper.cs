namespace FGMEmailSenderApp.Models.Interfaces
{
    public interface ILightCriptoHelper
    {
        string CriptName(string data);

        string CriptPhone(string data);

        string CriptNumber(int data);

        int CriptNumberToInt(int data);

        string CriptEmail(string email);
    }
}
