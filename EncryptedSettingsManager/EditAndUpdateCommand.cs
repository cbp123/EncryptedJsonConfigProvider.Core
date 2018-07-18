using System;
using System.IO;
using EncryptedConfiguration;
using Microsoft.Extensions.CommandLineUtils;

namespace EncryptedSettingsManager
{
    public class EditAndUpdateCommand : CommandBase
    {
        public const string NAME = "editAndUpdate";
        public EditAndUpdateCommand(string[] args, bool throwOnUnexpectedArg = true) : base(args, throwOnUnexpectedArg)
        {
            Name = NAME;
            Description = "Decrypt the encrypted settings file temporarily so that it can be modified. When the application is closed the file will be re-encrypted and then deleted.";
            AddCommonOptions();

            var inputFilenameOption =
                Option("-i|--inputFilename", "Full path of the encrypted settings file", CommandOptionType.SingleValue);
            var tempFilenameOption =
                Option("-tf|--tempFilename", "Full path of the temporary decrypted file to create", CommandOptionType.SingleValue);

            OnExecute(() =>
            {
                Console.WriteLine("Enter password");

                Password = GetPassword();

                Console.WriteLine("");

                if (string.IsNullOrEmpty(tempFilenameOption.Value()))
                {
                    Console.WriteLine("outputFilename argument not supplied");
                    return (int)Program.ExitCodes.InvalidArguments;
                }

                if (File.Exists(tempFilenameOption.Value()))
                {
                    Console.WriteLine($"Overwrite {tempFilenameOption.Value()}? (y/n)");

                    if (Console.ReadLine() != "y")
                        return (int)Program.ExitCodes.Cancelled;
                }

                try
                {
                    DecryptSettings(inputFilenameOption.Value(), tempFilenameOption.Value());

                    Console.WriteLine(
                        "Settings have been decrypted. Make changes as required, then hit enter to encrypt and delete the unencrypted file.");

                    Console.ReadLine();

                    Console.WriteLine($"Encrypt changes and overwrite {inputFilenameOption.Value()}? (y/n)");

                    if (Console.ReadLine() == "y")
                    {
                        EncryptSettings(tempFilenameOption.Value(), inputFilenameOption.Value());
                        Console.ReadKey();
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    if (File.Exists(tempFilenameOption.Value()))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Delete temporary file? (y/n)");
                        if (Console.ReadLine() == "y")
                        {
                            File.Delete(tempFilenameOption.Value());
                        }
                    }
                }

                return (int)Program.ExitCodes.Success;
            });
        }

        private void DecryptSettings(string inputFilename, string outputFilename)
        {
            void ProcessCryptoStream(Stream stream)
            {
                using (var fileStream = File.Create(outputFilename))
                {
                    stream.CopyTo(fileStream);
                }
            }

            EncryptedConfigurationProvider.Decrypt(
                ProcessCryptoStream, 
                inputFilename,
                Password);
        }

        private void EncryptSettings(string inputFilename, string outputFilename)
        {
            Console.WriteLine("Beginning encrypting settings.");
            EncryptedConfigurationUtilities.EncryptSettings(Password, inputFilename, null, outputFilename);
            Console.WriteLine("Finished encrypting settings. Press any key to exit.");
        }
    }
}
