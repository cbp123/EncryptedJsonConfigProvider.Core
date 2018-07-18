using System;
using System.Runtime.Serialization;

namespace EncryptedConfiguration
{
    public class InvalidEncryptedConfigurationException : Exception
    {
        public InvalidEncryptedConfigurationException()
        {
        }

        public InvalidEncryptedConfigurationException(string message) : base(message)
        {
        }

        public InvalidEncryptedConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidEncryptedConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}