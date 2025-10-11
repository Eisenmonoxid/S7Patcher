using System;
using System.Linq;
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
        private static GameVariant? Variant;
        private const string LauncherHash = "348783a3d9b93bb424b7054429cd4844";

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Title = "S7Patcher - github.com/Eisenmonoxid/S7Patcher";
            Console.Clear();

            string Version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            Console.WriteLine("[INFO] S7Patcher v" + Version + " currently running on " + RuntimeInformation.OSDescription.ToString());

            bool USE_DEBUG = args.Where(Element => Element.Contains("-debug")).Any();
            Console.WriteLine("[INFO] USE_DEBUG - Activated: " + USE_DEBUG.ToString() + "\n");

            FileStream Stream = GetFileStream(args);
            if (Stream == null)
            {
                Console.ReadKey();
                return;
            }

            // Main patching routine
            bool Result = HandlePatchingProcess(Stream, (GameVariant)Variant, USE_DEBUG, OpenDefinitionStream());
            // Main patching routine

            Console.WriteLine("\n[INFO] Finished!" + (!Result ? " One or more errors occured." : " No errors occured."));
            Console.WriteLine("[INFO] If you encounter any errors (or you want to give a thumbs up), please report on GitHub or Discord.");
            Console.WriteLine("[INFO] Press any key to exit ...");
            Console.ReadKey();
            return;
        }

        private static Stream OpenDefinitionStream()
        {
            Stream Definition;
            if (AskForDefinitionFile())
            {
                Definition = WebHandler.Instance.DownloadDefinitionFile().GetAwaiter().GetResult() ?? GetEmbeddedResourceDefinition();
            }
            else
            {
                Definition = GetEmbeddedResourceDefinition();
            }

            return Definition;
        }

        private static Stream GetEmbeddedResourceDefinition() => Assembly.GetExecutingAssembly().GetManifestResourceStream("S7Patcher.Definitions.Definitions.bin");

        private static bool HandlePatchingProcess(FileStream Stream, GameVariant Variant, bool Debug, Stream Definition)
        {
            if (new Patcher(Stream, Variant, Debug).PatchGameWrapper(Definition) == false)
            {
                Helpers.Instance.CloseFileStream(Stream);
                Console.WriteLine("[ERROR] Patching did not finish successfully! Aborting ...");
                return false;
            }

            string Path = Stream.Name;
            long Size = Stream.Length;
            Helpers.Instance.CloseFileStream(Stream);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
            {
                Console.WriteLine("[INFO] Non - Windows platform, skipping CheckSum calculation.");
                return true;
            }

            if (new CheckSumCalculator().WritePEHeaderFileCheckSum(Path, Size) == false)
            {
                Console.WriteLine("[ERROR] Could not update PE Header CheckSum!");
                return false;
            }

            return true;
        }

        private static bool AskForDefinitionFile()
        {
            Console.WriteLine("\n[INPUT] Download latest Definition file from the GitHub repository (Yes) or use local embedded resource (No)?\n(0 = Yes/1 = No):");
            int Input = Helpers.Instance.ConsoleReadWrapper();
            if (Input != '0')
            {
                Console.WriteLine("\n[INFO] Using embedded resource Definition file.");
                return false;
            }

            Console.WriteLine("\n[INFO] Downloading latest Definition file.");
            return true;
        }

        private static FileStream GetFileStream(string[] args)
        {
            FileStream Stream;
            string Filepath = args.Where(Element => Element.EndsWith(".exe")).FirstOrDefault();

            if (Filepath == default)
            {
                Console.WriteLine("[INPUT] Please input the executable path that you want to patch:");
                Filepath = Console.ReadLine();
            }

            if (File.Exists(Filepath) == false)
            {
                Console.WriteLine("[ERROR] File does not exist! Aborting ...");
                return null;
            }

            if (Helpers.Instance.CreateBackup(Filepath) == false)
            {
                Console.WriteLine("[ERROR] Could not create backup of file! Aborting ...");
                return null;
            }

            Stream = Helpers.Instance.OpenFileStream(Filepath);
            if (Stream == null)
            {
                Console.WriteLine("[ERROR] Could not open FileStream! Aborting ...");
                return null;
            }

            if (Helpers.Instance.GetFileHash(Stream).Equals(LauncherHash.ToLower()) == true)
            {
                Console.WriteLine("[INFO] Launcher found! Redirecting Filepath!");
                Helpers.Instance.CloseFileStream(Stream);
                return GetFileStream([Helpers.Instance.RedirectLauncherFilePath(Filepath)]);
            }

            Variant = Helpers.Instance.GetExecutableVariant(Stream);
            if (Variant != null)
            {
                Console.WriteLine("\n[INFO] Found Game Variant " + Variant.ToString() + ". \n[INFO] Going to patch file: " + Filepath);
                return Stream;
            }
            else
            {
                Console.WriteLine("[ERROR] Executable was not valid! Aborting ...");
                Helpers.Instance.CloseFileStream(Stream);
                return null;
            }
        }
    }
}
