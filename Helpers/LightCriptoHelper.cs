using FGMEmailSenderApp.Models.Interfaces;

namespace FGMEmailSenderApp.Helpers
{
    public class LightCriptoHelper : ILightCriptoHelper
    {
        #region PUBLIC ACCESS TO DATAHELPER

        public string CriptName(string data)
        {
            return LightCript(ArrayPushedIndividuallyChar(data));
        }

        public string CriptPhone(string data)
        {
            return LightCript(ArrayPushedIndividuallyChar(data));
        }

        public string CriptNumber(int data)
        {
            return LightCript(ArrayPushedIndividuallyChar(data.ToString()));
        }

        public int CriptNumberToInt(int data)
        {
            return Int32.Parse(LightCript(ArrayPushedIndividuallyChar(data.ToString())));
        }

        public string CriptEmail(string email)
        {
            return LightCriptEmail(ArrayPushedIndividuallyChar(email));
        }

        #endregion

        #region INTERNAL METHOD LIGHT CRIPT CREDENTIAL

        internal string LightCript(char[] data)
        {
            char[] charResult = new char[data.Length];

            int positionLast2Char = data.Length - 2;

            for (int i = 0; i < data.Length; i++)
            {
                if (i < positionLast2Char && i > 0)
                {
                    charResult[i] = '*';
                }
                else
                {
                    charResult[i] = data[i];
                }
            }

            return ConvertToString(charResult);

        }

        #endregion

        #region INTERNAL LIGHT CRIPTING CREDENTIAL EMAIL

        internal string LightCriptEmail(char[] dataEmail)
        {
            char[] charResult = new char[dataEmail.Length];

            IEnumerable<int> positionAt = this.ReturnPositionAt(dataEmail);
            
            for (int i = 0; i< dataEmail.Length; i++)
            {
                if (i > 1 && i < positionAt.FirstOrDefault()) 
                {
                    if (dataEmail[i].Equals('.'))
                    {
                        charResult[i] = '.';
                    }
                    else
                    {
                        charResult[i] = '*';
                    }
                }
                else
                {
                    charResult[i] = dataEmail[i];
                }
            }

            return ConvertToString(charResult);
        }

        #endregion

        #region PRIVATE TRANSFORM STRING INTO ARRAY BY ADDING EACH CHARACTER INDIVIDUALLY

        private char[] ArrayPushedIndividuallyChar(string data)
        {
            char[] arrayData = data.ToCharArray();

            return arrayData;
        }

        #endregion

        #region PRIVATE IDENTIFY POSITION OF AT @

        private IEnumerable<int> ReturnPositionAt(char[] data)
        {
            for(int i = 0; i < data.Length; i++)
            {
                if (data[i].Equals('@')) yield return i;
            }
        }

        #endregion

        #region PRIVATE CONVERT TO STRING ARRAY OF CHAR

        private string ConvertToString(char[] data)
        {
            string result = String.Empty;

            foreach (char character in data)
            {
                result += character;
            }

            return result;
        }

        #endregion
    }
}
