using System;
using Gini;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace PDNPortable
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles(); // This is only here for the error dialog box

            // Set the Registry Key 
            RegistryKey regKey;
            regKey = Registry.CurrentUser.CreateSubKey(@"Software\paint.net");

            // Load the Settings ini file
            string iniPath = @"settings.ini";
            if (File.Exists(iniPath))
            {
                string iniData;
                using (var streamReader = new StreamReader(iniPath))
                {
                    iniData = streamReader.ReadToEnd();
                }

                // Parse the ini file, and place values in the local registry
                foreach (var e in Ini.ParseFlatHash(iniData, (s, k) => s + "/" + k))
                {
                    string nameParsed = e.Key;
                    string name = nameParsed.Replace(".", "/");
                    string value = e.Value;

                    regKey.SetValue(name, value, RegistryValueKind.String);
                }
            }

            // Also set a registry value to disable updates
            regKey.SetValue("CHECKFORUPDATES", "0", RegistryValueKind.String);

            // Start paint.net, and wait for it to close
            string pdnexe = @"paint.net\PaintDotNet.exe";
            if (File.Exists(pdnexe))
                Process.Start(pdnexe).WaitForExit();
            else
                MessageBox.Show("Can not find the paint.net executable. Please copy it from an existing installation.", "PDN Portable");


            // Clean the local registry
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\paint.net");

            // Clean up the LocalAppData directory
            string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\paint.net";
            if (Directory.Exists(AppDataPath))
                Directory.Delete(AppDataPath, true);
        }
    }
}