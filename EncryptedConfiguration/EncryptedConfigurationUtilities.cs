using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace EncryptedConfiguration
{
    public static class EncryptedConfigurationUtilities
    {
        public static void EncryptSettings(SecureString password, string inputFilename, string inputJson, string outputFilename)
        {
            if (string.IsNullOrEmpty(inputFilename) == string.IsNullOrEmpty(inputJson))
                throw new ArgumentException("Either inputFilename or inputJson argument should be supplied, but not both");
            if (string.IsNullOrEmpty(outputFilename))
                throw new ArgumentNullException(nameof(outputFilename));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            // Generate data key
            var dataKeySalt = Convert.FromBase64String(Constants.DATA_KEY_SALT);
            
            var dataKey = new Rfc2898DeriveBytes(SecureStringToString(password), dataKeySalt);

            // Read plain text to encrypt
            var plainText = string.IsNullOrEmpty(inputFilename) ? inputJson : File.ReadAllText(inputFilename);
            
            // Validate json (this will throw an exception if the string is not JSON)
            JObject.Parse(plainText);

            // Generate IV for AES encryption
            var iv = new byte[Constants.IV_SIZE];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            // Do AES encryption
            byte[] encrypted;

            using (var aes = Aes.Create())
            {
                aes.Padding = PaddingMode.Zeros;
                aes.Mode = CipherMode.CBC;

                var encryptor = aes.CreateEncryptor(dataKey.GetBytes(Constants.DATA_KEY_SIZE), iv);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(cs))
                        {
                            swEncrypt.Write(plainText);
                        }
                        
                        encrypted = ms.ToArray();
                    }
                }
            }

            // Append IV to end of encrypted data (so it's always the last 16 bytes)
            var fileData = new byte[encrypted.Length+iv.Length];
            encrypted.CopyTo(fileData, 0);
            iv.CopyTo(fileData, encrypted.Length);

            File.WriteAllBytes(outputFilename, fileData);
        }

        public static string SecureStringToString(SecureString value) {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            IntPtr valuePtr = IntPtr.Zero;
            try {
                valuePtr = SecureStringMarshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            } finally {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }
}
