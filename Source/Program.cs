using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace S7Patcher.Source
{
    public enum GameVariant
    {
        ORIGINAL,
        HE_STEAM,
        HE_UBI
    }

    internal class Program
    {
        public static GameVariant? Variant;
        private const string LauncherHash = "348783a3d9b93bb424b7054429cd4844";

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Title = "S7Patcher - \"github.com/Eisenmonoxid/S7Patcher\"";
            Console.Clear();

            string Version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            Console.WriteLine("S7Patcher v" + Version + " currently running on " + RuntimeInformation.OSDescription.ToString());

            bool USE_DEBUG = false;
            if (args.Length >= 2 && args[1].Contains("-debug"))
            {
                USE_DEBUG = true;
            }
            Console.WriteLine("USE_DEBUG - Activated: " + USE_DEBUG.ToString() + "\n");

            FileStream Stream = GetFileStream(args);
            if (Stream == null)
            {
                Console.ReadKey();
                return;
            }

            HandlePatchingProcess(Stream, (GameVariant)Variant, USE_DEBUG); // Main patching routine

            Console.WriteLine("\nFinished!");
            Console.WriteLine("If you encounter any errors (or you want to give a thumbs up), please report on GitHub or Discord.");
            Console.WriteLine("Press any key to exit ...");
            Console.ReadKey();

            return;
        }

        public static void HandlePatchingProcess(FileStream Stream, GameVariant Variant, bool Debug)
        {
            new Patcher(Stream, Variant, Debug).PatchGameWrapper();
            new CheckSumCalculator().WritePEHeaderFileCheckSum(Stream);
        }

        public static FileStream GetFileStream(string[] args)
        {
            FileStream Stream;
            string Filepath;

            if (args.Length == 0)
            {
                Console.WriteLine("Please input the executable path that you want to patch:");
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

            Variant = Helpers.Instance.GetExecutableVariant(Stream);
            if (Variant != null)
            {
                Console.WriteLine("Found Game Variant " + Variant.ToString() + ". \nGoing to patch file: " + Filepath);
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
