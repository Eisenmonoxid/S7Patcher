using S7Patcher.Properties;
using System;
using System.IO;
using System.Text;

namespace S7Patcher.Source
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Title = "S7Patcher - \"github.com/Eisenmonoxid/S7Patcher\"";
            Console.TreatControlCAsInput = true;
            Console.Clear();

            FileStream Stream = GetFileStream(args);
            if (Stream == null)
            {
                Console.ReadKey();
                return; // Exit
            }

            ExecutePatch(Stream);

            Console.WriteLine("Finished!");
            Console.WriteLine("If you encounter any errors (or you want to give a thumbs up), please report on GitHub. Thanks in advance!");
            Console.ReadKey();

            return; // Exit
        }
        public static void ExecutePatch(FileStream Stream)
        {
            // Patch the executable "Settlers7R.exe"
            PatchFile(Stream);
            //AddModloaderToGame(Stream); -> Work In Progress

            Stream.Close();
            Stream.Dispose();

            // Patch the "Profiles.xml" file
            ReplaceDataInProfileFile();
        }
        public static void PatchFile(FileStream Stream)
        {
            Helpers.Instance.WriteToFile(Stream, 0x00D40D, [0xE8, 0xBC, 0x99, 0x68, 0x00, 0x90]);
            Helpers.Instance.WriteToFile(Stream, 0x696DCE, [0x55, 0x89, 0xE5, 0xC6, 0x05, 0x79, 0x5B, 0x0E, 0x01, 0x01, 0x89, 0xEC, 0x5D, 0xC3]);
            Helpers.Instance.WriteToFile(Stream, 0x1A978E, [0xEB]);
            Helpers.Instance.WriteToFile(Stream, 0x1A977C, [0x90, 0x90]);
            Helpers.Instance.WriteToFile(Stream, 0x64477C, [0xB0, 0x00]);
            Helpers.Instance.WriteToFile(Stream, 0x21929C, [0xB0, 0x00]);
            Helpers.Instance.WriteToFile(Stream, 0x219224, [0xB0, 0x00]);
            Helpers.Instance.WriteToFile(Stream, 0x195C34, [0xEB, 0x15]);
            Helpers.Instance.WriteToFile(Stream, 0x69000F, [0x94]);
            Helpers.Instance.WriteToFile(Stream, 0x58BC2E, [0x01]);
            Helpers.Instance.WriteToFile(Stream, 0x696D83, [0x90, 0x90, 0x90, 0x90, 0x90]);
            Helpers.Instance.WriteToFile(Stream, 0x696DC8, [0xE9, 0x0B, 0x03, 0x00, 0x00, 0x90]);
        }

        public static void ReplaceDataInProfileFile()
        {
            string ProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Settlers7", "Profiles.xml");
            if (File.Exists(ProfilePath) == false)
            {
                return;
            }

            UpdateProfileXML(ProfilePath);
        }
        public static void UpdateProfileXML(string Filepath)
        {
            string[] Lines;
            try
            {
                Lines = File.ReadAllLines(Filepath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            ushort[] Indizes = [0, 0];
            for (ushort Index = 0; Index < Lines.Length; Index++)
            {
                if (Lines[Index].Contains("<Titles>") && Lines[Index + 1].Contains("</TitleSystem>"))
                {
                    Indizes[0] = Index;
                }
                else if (Lines[Index].Contains("<LastSynchTime>"))
                {
                    Indizes[1] = Index;
                }
            }

            if (Indizes[0] != 0 && Indizes[1] != 0)
            {
                Lines[Indizes[0] - 1] = Resources.Branch;
                Lines[Indizes[0]] = Resources.Title;
                Lines[Indizes[1] + 2] = Resources.Year;

                try
                {
                    File.WriteAllLines(Filepath, Lines);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static FileStream GetFileStream(string[] args)
        {
            FileStream Stream;
            string Filepath;

            if (args.Length == 0)
            {
                Console.WriteLine("Please input the filepath that you want to patch:\r\n");
                Filepath = Console.ReadLine();
            }
            else
            {
                Filepath = args[0];
            }

            Console.WriteLine("Going to patch file: " + Filepath);
            if (File.Exists(Filepath) == false)
            {
                Console.WriteLine("ERROR - Passed file is not valid! Aborting ...");
                return null;
            }

            if (Helpers.Instance.CreateBackup(Filepath) == false)
            {
                Console.WriteLine("ERROR - Could not create backup of file! Aborting ...");
                return null;
            }

            Stream = Helpers.Instance.OpenFileStream(Filepath);
            if (Stream == null)
            {
                Console.WriteLine("ERROR - Could not open FileStream! Aborting ...");
                return null;
            }

            return Stream;
        }

        public static void AddModloaderToGame(FileStream Stream)
        {
            byte[] Bytes = [0x55, 0x8B, 0xEC, 0x81, 0xEC, 0x00, 0x03, 0x00, 0x00, 0x68, 0x00, 0x00, 0x00, 0x00, 0x68, 0x05, 0x00, 0x00, 0x00, 0x8D, 0x85, 0x80, 0xFD,
                0xFF, 0xFF, 0x50, 0x68, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x15, 0x30, 0xF8, 0xEB, 0x00, 0x8D, 0x95, 0x80, 0xFD, 0xFF, 0xFF, 0x52, 0x31, 0xC0, 0x89, 0x45, 0xFC, 0x89,
                0x45, 0xF8, 0x89, 0x45, 0xF4, 0x89, 0x45, 0xF0, 0x8D, 0x45, 0xF0, 0xE8, 0xD0, 0xE5, 0x97, 0xFF, 0x68, 0x70, 0xEC, 0xF2, 0x00, 0x8D, 0x85, 0xF0, 0xFF, 0xFF, 0xFF,
                0xE8, 0xE0, 0xE7, 0x97, 0xFF, 0xE8, 0x5B, 0xF2, 0xA9, 0xFF, 0x83, 0xC0, 0x5C, 0x68, 0x1C, 0x00, 0x00, 0x00, 0x68, 0x00, 0x00, 0x00, 0x00, 0x8D, 0x4D, 0xF0, 0x51,
                0x50, 0xE8, 0xD4, 0x3D, 0xF2, 0xFF, 0x31, 0xC0, 0x8B, 0x35, 0xF8, 0xF6, 0xEB, 0x00, 0x8B, 0x45, 0xF0, 0x50, 0xFF, 0xD6, 0x8B, 0x85, 0x80, 0xFD, 0xFF, 0xFF, 0x50,
                0xFF, 0xD6, 0x81, 0xC4, 0x00, 0x03, 0x00, 0x00, 0x89, 0xEC, 0x5D, 0xC3];

            Helpers.Instance.WriteToFile(Stream, 0x696DCE, Bytes);
            Helpers.Instance.WriteToFile(Stream, 0xB2D870, Encoding.ASCII.GetBytes("\\Settlers7\\Modloader\0\0"));

            Helpers.Instance.WriteToFile(Stream, 0x130552, [0xE8, 0x17, 0x69, 0x56, 0x00]);
            Helpers.Instance.WriteToFile(Stream, 0x696E6E, [0x55, 0x89, 0xE5, 0x66, 0x60, 0xE8, 0x56, 0xFF, 0xFF, 0xFF, 0x66, 0x61, 0x89, 0xEC, 0x5D, 0xE8, 0x5E, 0xBD, 0xA9, 0xFF, 0xC3]);
        }
    }
}
