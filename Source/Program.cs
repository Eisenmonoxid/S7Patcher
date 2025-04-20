using System;
using System.IO;

namespace S7Patcher.Source
{
    internal class Program
    {
        private const bool USE_DEBUG = false;
        private const string LauncherHash = "348783a3d9b93bb424b7054429cd4844";

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
                return;
            }

            HandlePatchingProcess(Stream, USE_DEBUG);

            Console.WriteLine("Press any key to exit ...");
            Console.ReadKey();
            return;
        }

        public static void HandlePatchingProcess(FileStream Stream, bool Debug)
        {
            Patcher Patcher = new(Stream);
            Patcher.PatchFile(Debug);
            Patcher.UpdateConfigurationFile("Profiles.xml");
            Patcher.UpdateConfigurationFile("Options.ini");

            Console.WriteLine("\nFinished!");
            Console.WriteLine("If you encounter any errors (or you want to give a thumbs up), please report on GitHub or Discord.");

            Stream.Close();
            Stream.Dispose();
        }

        public static FileStream GetFileStream(string[] args)
        {
            FileStream Stream;
            string Filepath;

            if (args.Length == 0)
            {
                Console.WriteLine("Please input the path to the executable that you want to patch:\n");
                Filepath = Console.ReadLine();
            }
            else
            {
                Filepath = args[0];
            }

            if (File.Exists(Filepath) == false)
            {
                Console.WriteLine("ERROR - File does not exist!");
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

            if (Helpers.Instance.GetFileHash(Stream).Equals(LauncherHash.ToLower()) == true)
            {
                Console.WriteLine("Launcher found! Redirecting Filepath!");

                Stream.Close();
                Stream.Dispose();
                return GetFileStream([Helpers.Instance.RedirectLauncherFilePath(Filepath)]);
            }
            else if (Helpers.Instance.IsExecutableValid(Stream) == true)
            {
                Console.WriteLine("Going to patch file: " + Filepath);
                return Stream;
            }
            else
            {
                Console.WriteLine("ERROR - Executable was not valid! Aborting ...");

                Stream.Close();
                Stream.Dispose();
                return null;
            }
        }
    }
}
