using System;
using System.IO;

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

            Patcher Patcher = new(Stream);
            if (Patcher.IsExecutableValid() == false)
            {
                Console.WriteLine("ERROR - Executable was not valid! Aborting ...");
            }
            else
            {
                Patcher.PatchFile();
                Patcher.ReplaceDataInProfileFile();

                Console.WriteLine("Finished!");
                Console.WriteLine("If you encounter any errors (or you want to give a thumbs up), please report on GitHub or Discord. Thanks in advance!");
            }

            Stream.Close();
            Stream.Dispose();

            Console.ReadKey();
            return; // Exit
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

            return Stream;
        }
    }
}
