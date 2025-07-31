using System;
using System.IO;

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

        public void AskForAffinity()
        {
            Console.WriteLine("\nUpdate Process Affinity? (Enables higher framerate and smoother performance)\n(0 = Yes/1 = No):");
            int Input = Console.Read();
            if (Input != '0')
            {
                Console.WriteLine("Skipping Affinity ...");
                return;
            };

            byte Mask;
            Console.WriteLine("\nInput Affinity Mask (physical cores the game should run on) as a hexadecimal value (Must be <= 2A): \n(e.g. 7 or F or A)");

            do
            {
                string Value = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(Value))
                {
                    Console.WriteLine("Input cannot be empty!");
                    continue;
                }

                if (byte.TryParse(Value, System.Globalization.NumberStyles.HexNumber, null, out Mask))
                {
                    break;
                }
                Console.WriteLine("Erroneous input value. Please try again!");
            }
            while (true);

            Console.WriteLine("Going to patch Affinity with value: 0x" + $"{Mask:X}");
            UpdateProcessAffinity(Mask);
        }

        private void UpdateProcessAffinity(byte Mask)
        {
            // TODO: HE support? Elevate process or thread priority?
            Helpers.Instance.WriteToFile(GlobalStream, 0x62F0AE, [0x55, 0x8B, 0xEC, 0x68, 0x38, 0x73, 0xF2, 0x00, 0xFF, 
                0x15, 0x70, 0xF1, 0xEB, 0x00, 0x68, 0xC8, 0x25, 0xF3, 0x00, 0x50, 0xFF, 0x15, 0x24, 0xF1, 0xEB, 0x00, 
                0x85, 0xC0, 0x74, 0x10, 0x90, 0x90, 0x90, 0x89, 0xC3, 0x6A, Mask, 0xFF, 0x15, 0x3C, 0xF2, 0xEB, 0x00, 
                0x50, 0xFF, 0xD3, 0x8B, 0xE5, 0x5D, 0xC3, 0x90]); // Create new function to set process affinity

            Helpers.Instance.WriteToFile(GlobalStream, 0xB311C8, [0x53, 0x65, 0x74, 0x50, 0x72, 0x6F, 0x63, 0x65, 0x73, 0x73, 0x41, 
                0x66, 0x66, 0x69, 0x6E, 0x69, 0x74, 0x79, 0x4D, 0x61, 0x73, 0x6B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]); // Update function name

            Helpers.Instance.WriteToFile(GlobalStream, 0x00D944, [0xE8, 0x65, 0x17, 0x62, 0x00, 0xEB, 0x08, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90]); // Override Thread Affinity & Jump
        }
    }
}
