using System;
using System.Security.Cryptography;

namespace EncryptedSettingsManager
{
    public class GenerateSaltCommand : CommandBase
    {
        public const string NAME = "generateSalt";
        public GenerateSaltCommand(string[] args, bool throwOnUnexpectedArg = true) : base(args, throwOnUnexpectedArg)
        {
            Name = "generateSalt";
            Description = "Returns a salt for use with the AES encryption algorithm used internally.";
            AddCommonOptions();

            OnExecute(() =>
            {
                GenerateSalt();
                return (int) Program.ExitCodes.Success;
            });
        }

        private static void GenerateSalt()
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            Console.WriteLine(Convert.ToBase64String(salt));
        }
    }
}
