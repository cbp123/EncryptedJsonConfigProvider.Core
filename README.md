# EncryptedJsonConfigProvider.Core

This code can be used in a .NET Core application in which you wish to store configuration settings in an encrypted JSON file on disk, and then load the settings into an IConfigurationRoot using the Microsoft.Extensions.Configuration library. At runtime you would require your users to enter a password in order to decrypt the configuration.

A separate console application (EncryptedSettingsManager) is included that can be used for performing the encryption, and for decrypting the contents of the JSON file so that it can be edited.

Note that while this approach is not 100% secure, I believe there are a few use cases where it will be appropriate and more robust than other common alternatives. I am keen to here the developer community's insights into what vulnerabilities exist, so feel free to raise as issues.

## Before using

The salt used for encryption is currently hardcoded as constant (see https://github.com/cbp123/EncryptedJsonConfigProvider.Core/blob/master/EncryptedConfiguration/Constants.cs). I suggest that you change this salt to something else (any random letters or numbers will work, or alternatively I have included a utility in EncryptedSettingsManager that will generate one for you). I am still trying to work out the best way to have the salt supplied to the application in a convenient manner.

## Usage example

### Step 1 - Create a JSON file containing your configuration settings

Save the file on your computer somewhere, such as in C:\temp\
Note that at this point the information is exposed and vulnerable.

### Step 2 - Encrypt the JSON file using the EncryptedSettingsManager console application included with this repository

An example command is:
```
.\encryptedSettingsManager.exe encrypt -i "C:\temp\myConfig.json" -o "C:\MyApp\Config\myConfig.encrypted"
```

The application will ask you to delete the original file, which I suggest you do.

### Step 3 - Add code to your application to ask the user for a password

Here is an example appropriate for a console application:
```cs
    protected SecureString GetPassword()
    {
        var pwd = new SecureString();

        var passwordOption = this.Options.FirstOrDefault(x => x.LongName == PasswordOption.LONG_NAME);
        if (passwordOption != null 
            && passwordOption.HasValue() 
            && !string.IsNullOrEmpty(passwordOption.Value()))
        {
            foreach (var c in passwordOption.Value())
            {
                pwd.AppendChar(c);
            }

            return pwd;
        }

        Console.WriteLine("Enter password");

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
```

### Step 4 - Decrypt and load the configuration into memory using the user supplied password

For example:

```cs
Configuration = new ConfigurationBuilder()
    .Add(new EncryptedConfigurationSource(password, @"C:\MyApp\Config\myConfig.encrypted")
    .Build();
```

### Step 5 - To subsequently make changes to the unencrypted JSON file, use the EncryptedSettingsManager console app

Running this command will decrypt the encrypted file and place it in your temp directory where it can be edited:

```
.\encryptedSettingsManager.exe editAndUpdate -i "C:\MyApp\Config\myConfig.encrypted" -tf "C:\temp\myConfig.json"
```

Once you have finished editing the file, it will be encrypted again and the temporary file will be deleted.

## Dependencies
* .NET Core 2.1.x
  * Microsoft.Extensions.Configuration 2.0.0
  * Microsoft.Extensions.Configuration.Json 2.0.0
  * Newtonsoft.Json 10.0.3
