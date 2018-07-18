using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;

namespace EncryptedConfiguration
{
    // TODO Check out https://github.com/Narochno/Narochno.Credstash for an alternative 

    public class EncryptedConfigurationProvider : JsonConfigurationProvider
    {
        private readonly SecureString _password;
        private readonly string _file;

        public EncryptedConfigurationProvider(SecureString password, string file)
            : base(new JsonConfigurationSource
            {
                Path = file,
                ReloadOnChange = false,
            })
        {
            _password = password;
            _file = file;
        }

        public override void Load()
        {
            Decrypt(stream => base.Load(stream), _file, _password);
        }

        public static void Decrypt(Action<Stream> processCryptoStream, string file, SecureString password)
        {
            byte[] dataKeySalt = Convert.FromBase64String(Constants.DATA_KEY_SALT);

            var passwordString = EncryptedConfigurationUtilities.SecureStringToString(password);

            var dataKey = new Rfc2898DeriveBytes(passwordString, dataKeySalt);

            var dataKeyBytes = dataKey.GetBytes(Constants.DATA_KEY_SIZE);

            var fileBytes = File.ReadAllBytes(file);
            var encryptedData = fileBytes.Take(fileBytes.Length - Constants.IV_SIZE).ToArray();
            var iv = fileBytes.Skip(fileBytes.Length - Constants.IV_SIZE).ToArray();

            using (var aes = Aes.Create())
            {
                aes.Padding = PaddingMode.Zeros;
                aes.Mode = CipherMode.CBC;
                
                var decryptor = aes.CreateDecryptor(dataKeyBytes, iv);

                using (var fileStream = new MemoryStream(encryptedData))
                {
                    using (var cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read))
                    {
                        try
                        {
                            processCryptoStream(cryptoStream);
                        }
                        catch (JsonException ex)
                        {
                            throw new InvalidEncryptedConfigurationException("Could not parse encrypted configuration file. This may mean you entered the password incorrectly. Check inner exception for full details.", ex);
                        }
                    }
                }
            }
        }
    }
}
