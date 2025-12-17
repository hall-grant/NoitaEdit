/*
 * Set up file handling
 * Set up config directory and config file
*/

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

class Configure
{
    // ****** GETTERS ******

    public static string GetConfigSavePath()
    {
        string configPath = EnsureConfigFileExists();
        string savePath;
        string key = "savePath";
        try{
            return GetConfigValue(configPath, key);
        }
        catch (KeyNotFoundException knf)
        {
            Console.Error.WriteLine(knf.Message);

            try{
                savePath = GetDefaultSavePath();
            }
            catch(PlatformNotSupportedException pns)
            {
                Console.Error.WriteLine(pns.Message);
                Console.WriteLine("Please enter the path to your save file: \n");

                try
                {
                    savePath = GetUserSavePath();
                }
                catch(InvalidOperationException ioe)
                {
                    Console.Error.WriteLine(ioe.Message);
                    throw;
                }
                
            }
            SetConfigField(configPath, key, savePath);
            return savePath;

        }

        
    }

    static string GetDefaultSavePath()
    {
        /* Default path to save files:
         * Windows: C:/users/[windows_username]/AppData/LocalLow/Nolla_Games_Noita/
         * Linux: ~/.steam/steam/steamapps/compatdata/881100/pfx/drive_c/users/[windows_username]/AppData/LocalLow/Nolla_Games_Noita/
        */
        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(homePath, @"AppData\LocalLow\Nolla_Games_Noita\");
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(homePath, @".steam/steam/steamapps/compatdata/881100/pfx/drive_c/users/", 
            Path.GetFileName(homePath), @"AppData/LocalLow/Nolla_Games_Noita/");
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported OS");
        }
        
    }

    static string GetUserSavePath()
    {
        Console.WriteLine("Can't find path to the directory of save files. Enter path: \n");
        while (true)
        {
            string? userSavePath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userSavePath))
            {
                Console.Error.WriteLine("No input received. Please enter the path to Noita saves: ");
                continue;
            }
            return userSavePath;
        }
    }

    

    public static string EnsureConfigFileExists()
    {
        string NoitaConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NoitaEdit");
        string NoitaConfigFile = Path.Combine(NoitaConfigDir,"setup.cf");
        if (Directory.Exists(NoitaConfigDir))
        {
            if (File.Exists(NoitaConfigFile))
            {
                return NoitaConfigFile;
            }
            else
            {
                using var _ = File.Create(NoitaConfigFile); // "using var _ = " disposes of FileStream.
                return NoitaConfigFile;
            }
        }
        else
        {
            Directory.CreateDirectory(NoitaConfigDir);
            using var _ = File.Create(NoitaConfigFile);
            return NoitaConfigFile;
        }
        
    }

    public static string GetConfigValue(string configPath, string key)
    {
        foreach(string line in File.ReadLines(configPath))
        {
            string trimmed = line.Trim();
            
            // skip empty
            if (trimmed.Length == 0) continue;

            int indexEquals = trimmed.IndexOf('=');
            if (indexEquals < 0) continue;

            string foundKey = trimmed[..indexEquals].Trim();
            if(!string.Equals(foundKey, key)) continue;

            string value = trimmed[(indexEquals+1)..].Trim();

            // remove quotes (if present)
            if(value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
            {
                value = value.Substring(1, value.Length - 2);
            }
            return value;
        }
        throw new KeyNotFoundException($"Key '{key}' not found in config file '{configPath}'.\n");
    }


    // ****** SETTERS ******

    public static void SetConfigField(string configPath, string key, string value)
    {
        // Change value if key exists
        // Make key and value if not

        List<string> lines;
        if (File.Exists(configPath))
        {
            lines = File.ReadAllLines(configPath).ToList();
        }
        else
        {
            lines = new List<string>();
        }
        bool updated = false;
        for (int i = 0; i < lines.Count; i++)
        {
            string trimmed = lines[i].Trim();
            if(trimmed.Length == 0) continue;
            
            int indexEquals = trimmed.IndexOf('=');
            if(indexEquals < 0) continue;

            string foundKey = trimmed[..indexEquals].Trim();
            if(!string.Equals(foundKey, key)) continue;

            // replace value
            lines[i] = $"{key} = {value}";
            updated = true;
            break;
        }
        
        if (!updated) lines.Add($"{key} = \"{value}\"");

        File.WriteAllLines(configPath, lines);
    }

}

