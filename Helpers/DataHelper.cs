namespace FGMEmailSenderApp.Helpers
{
    public class DataHelper
    {
        #region PUBLIC PROPERTIES
        
        public string result { get; set; }

        #endregion

        #region PUBLIC ACCESS TO DATAHELPER
        public DataHelper(string data)
        {
            result = LightCript(ArrayPushedIndividuallyChar(data));
        }

        public DataHelper(int data)
        {
            result = LightCript(ArrayPushedIndividuallyChar(data.ToString()));
        }

        public DataHelper(char[] data)
        {
            result = LightCript(data);
        }

        public DataHelper() { }

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
                if (i < positionLast2Char)
                {
                    charResult[i] = '*';
                }
                else
                {
                    charResult[i] = data[i];
                }
            }

            string result = charResult.ToString();

            return result;

        }

        #endregion

        #region INTERNAL LIGHT CRIPTING CREDENTIAL EMAIL

        internal string LightCriptEmail(char[] dataEmail)
        {
            char[] charResult = new char[dataEmail.Length];

            int positionAt = this.ReturnPositionAt(dataEmail);
            
            for (int i = 0; i< dataEmail.Length; i++)
            {
                if (i > 1 && i < positionAt) 
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
            }

            string result = charResult.ToString();

            return result;
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

        private int ReturnPositionAt(char[] data)
        {
            int positionAt = 0;

            for(int i = 0; i < data.Length; i++)
            {
                if (data[i].Equals('@')) positionAt = i;
            }

            return positionAt;
        }

        #endregion
    }
}
