using S7Patcher.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace S7Patcher.Source
{
    internal class Patcher(FileStream Stream)
    {
        private readonly FileStream GlobalStream = Stream;
        public void PatchOriginalRelease(bool Debug)
        {
            Helpers.Instance.WriteToFile(GlobalStream, 0x00D40D, [0xE8, 0xBC, 0x99, 0x68, 0x00, 0x90]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x696DCE, [0x55, 0x89, 0xE5, 0xC6, 0x05, 0x79, 0x5B, 0x0E, 0x01, 0x01, 0x89, 0xEC, 0x5D, 0xC3]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x1A978E, [0xEB]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x1A977C, [0x90, 0x90]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x64477C, [0xB0, 0x00]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x21929C, [0xB0, 0x00]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x219224, [0xB0, 0x00]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x195C34, [0xEB, 0x15]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x69000F, [0x94]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x58BC2E, [0x01]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x696D83, [0x90, 0x90, 0x90, 0x90, 0x90]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x696DC8, [0xE9, 0x0B, 0x03, 0x00, 0x00, 0x90]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x62F0A9, [0xE9, 0xF2, 0x00, 0x00, 0x00, 0x90]);

            if (Debug)
            {
                Helpers.Instance.WriteToFile(GlobalStream, 0x00D2D9, [0x90, 0x90]); // Always show message before startup happens
            }
        }

        public void PatchHistoryEdition(GameVariant Variant, bool Debug)
        {
            if (Variant == GameVariant.HE_STEAM)
            {
                Helpers.Instance.WriteToFile(GlobalStream, 0x13CFEC, [0x94]);

                if (Debug)
                {
                    Helpers.Instance.WriteToFile(GlobalStream, 0xAC5435, [0x90, 0x90]); // Always show message before startup happens
                }
            }
            else if (Variant == GameVariant.HE_UBI)
            {
                Helpers.Instance.WriteToFile(GlobalStream, 0x13D9FC, [0x94]);

                if (Debug)
                {
                    Helpers.Instance.WriteToFile(GlobalStream, 0xAC5D4A, [0x90, 0x90]); // Always show message before startup happens
                }
            }
        }

        public void UpdateConfigurationFile(string Name)
        {
            string Filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Settlers7", Name);
            if (File.Exists(Filepath) == false)
            {
                do
                {
                    Console.WriteLine("\n" + Filepath + " not found!\nPlease input the path to the " + Name + " file:\n(Input skip to skip file patching)");
                    Filepath = Console.ReadLine();

                    if (Filepath == "skip")
                    {
                        Console.WriteLine("Skipping file patching ...");
                        return;
                    }
                    else if (File.Exists(Filepath))
                    {
                        break;
                    }
                }
                while (true);
            }

            Console.WriteLine("Going to patch file: " + Filepath);
            if (Name == "Profiles.xml")
            {
                Helpers.Instance.UpdateProfileXML(Filepath);
            }
            else if (Name == "Options.ini")
            {
                Helpers.Instance.UpdateEntriesInOptionsFile(Filepath);
            }
        }
    }
}
