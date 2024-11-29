using System;
using System.IO;

namespace S7Patcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.Title = "S7Patcher - Eisenmonoxid";
            Console.TreatControlCAsInput = true;
            Console.Clear();

            if (args.Length == 0)
            {
                Console.WriteLine("S7Patcher: ERROR - No valid file passed as argument! Aborting ...");
                Console.ReadKey();
                return;
            }
            else
            {
                string Filepath = args[0];
                if (!File.Exists(Filepath))
                {
                    Console.WriteLine("S7Patcher: ERROR - No valid file passed as argument! Aborting ...");
                    Console.ReadKey();
                    return;
                }

                if (!CreateBackup(Filepath))
                {
                    Console.WriteLine("S7Patcher: ERROR - Could not create backup of file! Aborting ...");
                    Console.ReadKey();
                    return;
                }

                FileStream Stream = OpenFileStream(Filepath);
                if (Stream == null)
                {
                    Console.WriteLine("S7Patcher: ERROR - Could not open FileStream! Aborting ...");
                    Console.ReadKey();
                    return;
                }
                else
                {
                    PatchFile(ref Stream);
                    Stream.Close();
                    Stream.Dispose();
                }
            }

            Console.WriteLine("S7Patcher: Finished successfully!");
            Console.ReadKey();
            Environment.Exit(0);
        }
        public static void PatchFile(ref FileStream Stream)
        {
            WriteToFile(ref Stream, 0xD40E,   new byte[] {0x2D});
            WriteToFile(ref Stream, 0x1A978E, new byte[] {0xEB});
            WriteToFile(ref Stream, 0x1A977C, new byte[] {0x90, 0x90});
            WriteToFile(ref Stream, 0x64477C, new byte[] {0xB0, 0x00});
            WriteToFile(ref Stream, 0x21929C, new byte[] {0xB0, 0x00});
            WriteToFile(ref Stream, 0x219224, new byte[] {0xB0, 0x00});
            WriteToFile(ref Stream, 0x195C34, new byte[] {0xEB, 0x15});
        }
        public static bool CreateBackup(string Path)
        {
            try
            {
                File.Copy(Path, Path + "_BACKUP", false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }
        public static void WriteToFile(ref FileStream Stream, long Position, byte[] Bytes)
        {
            Stream.Position = Position;
            try
            {
                Stream.Write(Bytes, 0, Bytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public static FileStream OpenFileStream(string Path)
        {
            FileStream Stream;
            try
            {
                Stream = new FileStream(Path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
            return Stream;
        }
    }
}
