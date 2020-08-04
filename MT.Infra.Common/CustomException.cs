using System;

namespace MT.Infra.Common
{
    [Serializable]
    public class CustomException : ApplicationException
    {
        public CustomException(string message) : base(message)
        {

        }
    }    
}
