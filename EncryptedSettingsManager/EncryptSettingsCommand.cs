using System;
using System.IO;
using EncryptedConfiguration;
using Microsoft.Extensions.CommandLineUtils;

namespace EncryptedSettingsManager
{
    public class EncryptSettingsCommand : CommandBase
    {
        public const string NAME = "encrypt";
        public EncryptSettingsCommand(string[] args, bool throwOnUnexpectedArg = true) : base(args, throwOnUnexpectedArg)
        {
            Name = NAME;
            Description = "Encrypt the specified file.";
            AddCommonOptions();

            var inputFilenameOption =
                Option("-i|--inputFilename", "Full path of the configuration file to encrypt", CommandOptionType.SingleValue);
            var inputJsonOption =
                Option("-ij|--inputJson", "JSON string to encrypt", CommandOptionType.SingleValue);
            var outputFilenameOption =
                Option("-o|--outputFilename", "Full path of the output encrypted file to create", CommandOptionType.SingleValue);

            OnExecute(() =>
            {
                Console.WriteLine("Enter password");

                Password = GetPassword();

                Console.WriteLine("");

                if (string.IsNullOrEmpty(inputFilenameOption.Value())
                    == string.IsNullOrEmpty(inputJsonOption.Value()))
                {
                    Console.WriteLine("Either inputFilename or inputJson argument should be supplied, but not both");
                    return (int)Program.ExitCodes.InvalidArguments;
                }

                if (string.IsNullOrEmpty(outputFilenameOption.Value()))
                {
                    Console.WriteLine("outputFilename argument not supplied");
                    return (int)Program.ExitCodes.InvalidArguments;
                }

                if (File.Exists(outputFilenameOption.Value()))
                {
                    Console.WriteLine($"Overwrite {outputFilenameOption.Value()}? (y/n)");

                    if (Console.ReadLine() != "y")
                        return (int)Program.ExitCodes.Cancelled;
                }

                EncryptSettings(inputFilenameOption.Value(), inputJsonOption.Value(), outputFilenameOption.Value());

                Console.WriteLine($"Delete {inputFilenameOption.Value()}? (y/n)");

                if (Console.ReadLine() == "y")
                {
                    File.Delete(inputFilenameOption.Value());
                }

                Console.WriteLine("Press any key to exit.");

                Console.ReadKey();

                return (int)Program.ExitCodes.Success;
            });
        }

        private void EncryptSettings(string inputFilename, string inputJson, string outputFilename)
        {
            Console.WriteLine("Beginning encrypting settings.");
            EncryptedConfigurationUtilities.EncryptSettings(Password, inputFilename, inputJson, outputFilename);
            Console.WriteLine("Finished encrypting settings.");
        }
    }
}
