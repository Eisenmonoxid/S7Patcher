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

    public enum ConsoleColorType
    {
        INFO,
        ERROR,
        SUCCESS,
        INPUT
    }

    internal class Program
    {
        private static GameVariant? Variant;
        private static bool UseCheckSum = true;
        private const string LauncherHash = "348783a3d9b93bb424b7054429cd4844";

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Title = "S7Patcher - github.com/Eisenmonoxid/S7Patcher";
            Console.Clear();

            string Version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "S7Patcher v" + Version + " currently running on " + RuntimeInformation.OSDescription.ToString());

            bool USE_DEBUG = args.Any(Element => Element.Contains("-debug"));
            Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "USE_DEBUG - Activated: " + USE_DEBUG.ToString());

            UseCheckSum = !args.Any(Element => Element.Contains("-skipchecksum"));

            FileStream Stream = GetFileStream(args);
            if (Stream == null)
            {
                try {Console.ReadKey();} catch {} // Console.ReadKey can throw exceptions in some environments
                return;
            }

            // Main patching routine
            bool Result = HandlePatchingProcess(Stream, (GameVariant)Variant, USE_DEBUG, OpenDefinitionStream());
            // Main patching routine

            Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "Finished!" + (!Result ? " One or more errors occured." : " No errors occured."));
            Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "If you encounter any errors (or you want to give a thumbs up), please report on GitHub or Discord.");
            Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "Press any key to exit ...");

            try {Console.ReadKey();} catch {} // Console.ReadKey can throw exceptions in some environments
            return;
        }

        private static Stream OpenDefinitionStream()
        {
            string Name = "S7Patcher.Definitions.Definitions.bin";
            Stream Definition;

            if (AskForDefinitionFile())
            {
                Definition = WebHandler.Instance.DownloadDefinitionFile().GetAwaiter().GetResult() ?? Helpers.Instance.GetEmbeddedResourceDefinition(Name);
            }
            else
            {
                Definition = Helpers.Instance.GetEmbeddedResourceDefinition(Name);
            }

            return Definition;
        }

        private static bool HandlePatchingProcess(FileStream Stream, GameVariant Variant, bool Debug, Stream Definition)
        {
            Patcher Main;
            try
            {
                Main = new(Stream, Definition, Variant, Debug);
            }
            catch
            {
                Helpers.Instance.CloseFileStream(Stream);
                return false;
            }

            if (!Main.PatchGameWrapper())
            {
                Helpers.Instance.CloseFileStream(Stream);
                Helpers.Instance.WriteWrapper(ConsoleColorType.ERROR, "Patching did not finish successfully! Aborting ...");
                return false;
            }

            string Path = Stream.Name;
            long Size = Stream.Length;
            Helpers.Instance.CloseFileStream(Stream);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !UseCheckSum)
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "Skipping CheckSum calculation.");
                return true;
            }

            if (!new CheckSumCalculator().WritePEHeaderFileCheckSum(Path, Size))
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.ERROR, "Could not update PE Header CheckSum!");
                return false;
            }

            return true;
        }

        private static bool AskForDefinitionFile()
        {
            Helpers.Instance.WriteWrapper(ConsoleColorType.INPUT, 
                "Download latest Definition file from the GitHub repository (Yes) or use local embedded resource (No)?\n(0 = Yes/1 = No):");

            string Input = Console.ReadLine();
            if (Input != "0")
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "Using embedded resource Definition file.");
                return false;
            }

            Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "Downloading latest Definition file.");
            return true;
        }

        private static FileStream GetFileStream(string[] args)
        {
            FileStream Stream;
            string Filepath = args.FirstOrDefault(Element => Element.EndsWith(".exe"));

            if (Filepath == default)
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.INPUT, "Please input the executable path that you want to patch:");
                Filepath = Console.ReadLine();
            }

            if (!File.Exists(Filepath))
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.ERROR, "File does not exist! Aborting ...");
                return null;
            }

            if (!Helpers.Instance.CreateBackup(Filepath))
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.ERROR, "Could not create backup of file! Aborting ...");
                return null;
            }

            Stream = Helpers.Instance.OpenFileStream(Filepath);
            if (Stream == null)
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.ERROR, "Could not open FileStream! Aborting ...");
                return null;
            }

            if (Helpers.Instance.GetFileHash(Stream).Equals(LauncherHash.ToLower()))
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "Launcher found! Redirecting Filepath!");
                Helpers.Instance.CloseFileStream(Stream);
                return GetFileStream([Helpers.Instance.RedirectLauncherFilePath(Filepath)]);
            }

            Variant = Helpers.Instance.GetExecutableVariant(Stream);
            if (Variant != null)
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "Found Game Variant " + Variant.ToString() + ".");
                Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "Going to patch file: " + Filepath);
                return Stream;
            }
            else
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.ERROR, "Executable is not valid! Aborting ...");
                Helpers.Instance.CloseFileStream(Stream);
                return null;
            }
        }
    }
}
