using S7Patcher.Properties;
using System;
using System.IO;
using System.Linq;
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

            ExecutePatch(Stream); // Main patching function

            Console.WriteLine("Finished!");
            Console.WriteLine("If you encounter any errors (or you want to give a thumbs up), please report on GitHub or Discord. Thanks in advance!");
            Console.ReadKey();

            return; // Exit
        }

        public static void ExecutePatch(FileStream Stream)
        {
            // Patch the executable "Settlers7R.exe"
            PatchFile(Stream);

            Stream.Close();
            Stream.Dispose();

            // Patch the "Profiles.xml" file
            ReplaceDataInProfileFile();
        }

        public static bool IsExecutableValid(FileStream Stream)
        {
            byte[] Identifier = [0x8B, 0x01];
            byte[] Result = new byte[Identifier.Length];

            Stream.Position = 0x00D24C;
            Stream.ReadExactly(Result);

            return Result.SequenceEqual(Identifier);
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
                Console.WriteLine(ProfilePath + " does not exist! Skipping .xml file patch ...");
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

            for (ushort Index = 0; Index < Lines.Length; Index++)
            {
                if (Lines[Index].Contains("<Titles>") && Lines[Index + 1].Contains("</TitleSystem>"))
                {
                    Lines[Index - 1] = Resources.Branch;
                    Lines[Index] = Resources.Title;
                }
                else if (Lines[Index].Contains("<LastSynchTime>"))
                {
                    Lines[Index + 2] = Resources.Year;
                }
            }

            try
            {
                File.WriteAllLines(Filepath, Lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static FileStream GetFileStream(string[] args)
        {
            FileStream Stream;
            string Filepath;

            if (args.Length == 0)
            {
                Console.WriteLine("Please input the path to the executable that you want to patch:\r\n");
                Filepath = Console.ReadLine();
            }
            else
            {
                Filepath = args[0];
            }

            Console.WriteLine("Going to patch file: " + Filepath);
            if (File.Exists(Filepath) == false)
            {
                Console.WriteLine("ERROR - File does not exist! Aborting ...");
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

            if (IsExecutableValid(Stream) == false)
            {
                Console.WriteLine("ERROR - Executable was not valid! Aborting ...");
                return null;
            }

            return Stream;
        }
    }
}
