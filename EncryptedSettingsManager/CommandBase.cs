using System;
using System.Security;
using Microsoft.Extensions.CommandLineUtils;

namespace EncryptedSettingsManager
{
    public abstract class CommandBase  : CommandLineApplication
    {
        protected string[] ProgramArgs { get; }

        protected SecureString Password { get; set; }

        protected CommandBase(string[] args, bool throwOnUnexpectedArg = true) : base(throwOnUnexpectedArg)
        {
            ProgramArgs = args;
        }

        /// <summary>
        /// Keeping the password in a SecureString is actually overkill, because it is the configuration settings that we are ultimately trying to protect, and they will be decrypted
        /// and therefore retrievable from memory anyway.
        /// However, just for fun I've left the password in a SecureString.
        /// </summary>
        protected static SecureString GetPassword()
        {
            var pwd = new SecureString();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.RemoveAt(pwd.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    pwd.AppendChar(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd;
        }

        /// <summary>
        /// Adds common command line options such as 'help' and 'config'
        /// </summary>
        protected void AddCommonOptions()
        {
            HelpOption("-?|-h|--help");
        }
    }
}
