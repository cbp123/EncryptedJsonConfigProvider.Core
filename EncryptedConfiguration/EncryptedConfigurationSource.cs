using System.Security;
using Microsoft.Extensions.Configuration;

namespace EncryptedConfiguration
{
    public class EncryptedConfigurationSource: IConfigurationSource
    {
        private readonly SecureString _password;
        private readonly string _file;

        public EncryptedConfigurationSource(SecureString password, string file)
        {
            _password = password;
            _file = file;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EncryptedConfigurationProvider(_password, _file);
        }
    }
}