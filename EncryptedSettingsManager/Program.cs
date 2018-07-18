using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace EncryptedSettingsManager
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootApp = new CommandLineApplication();
            rootApp.HelpOption("-?|-h|--help");

            rootApp.Commands.Add(new EncryptSettingsCommand(args));

            rootApp.Commands.Add(new EditAndUpdateCommand(args));

            rootApp.Commands.Add(new GenerateSaltCommand(args));

            rootApp.OnExecute(() =>
            {
                Console.WriteLine(rootApp.GetHelpText());
                return (int)ExitCodes.InvalidArguments;
            });

            int exitCode;

            try
            {
                exitCode = rootApp.Execute(args);
            }
            catch (TaskCanceledException)
            {
                exitCode = (int)ExitCodes.Cancelled;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                exitCode = (int) ExitCodes.Exception;
                Console.ReadLine();
            }

            Environment.Exit(exitCode);
        }

                
        public enum ExitCodes
        {
            Success = 0,
            InvalidArguments = 1,
            Exception = 2,
            Cancelled = 3
        }
    }
}
