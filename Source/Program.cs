﻿using S7Patcher.Source;
using System;
using System.IO;

namespace S7Patcher
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

            FileStream Stream = HandleInput(args);
            if (Stream == null)
            {
                Console.ReadKey();
                return; // Exit
            }

            // Patch the executable "Settlers7R.exe"
            PatchFile(ref Stream);
            Stream.Close();
            Stream.Dispose();

            // Patch the "Profiles.xml" file
            ReplaceTitleDataInProfileFile();

            Console.WriteLine("S7Patcher: Finished successfully!");
            Console.WriteLine("S7Patcher: If you encounter any errors (or you want to give a thumbs up), please report on GitHub. Thank you in advance!");
            Console.ReadKey();

            return; // Exit
        }
        public static void ReplaceTitleDataInProfileFile()
        {
            string SettlersPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Settlers7");
            string ProfilePath = Path.Combine(SettlersPath, "Profiles.xml");

            if (File.Exists(ProfilePath) == false)
            {
                return;
            }

            try
            {
                Helpers.Instance.WriteToXMLFile(ProfilePath);
            }
            catch (Exception ex)
            { 
                Console.WriteLine("ReplaceTitleDataInProfileFile - ERROR: " + ex.ToString());
            }
        }
        public static void PatchFile(ref FileStream Stream)
        {
            Helpers.Instance.WriteToFile(ref Stream, 0x00D40E, new byte[] {0x2D});
            Helpers.Instance.WriteToFile(ref Stream, 0x1A978E, new byte[] {0xEB});
            Helpers.Instance.WriteToFile(ref Stream, 0x1A977C, new byte[] {0x90, 0x90});
            Helpers.Instance.WriteToFile(ref Stream, 0x64477C, new byte[] {0xB0, 0x00});
            Helpers.Instance.WriteToFile(ref Stream, 0x21929C, new byte[] {0xB0, 0x00});
            Helpers.Instance.WriteToFile(ref Stream, 0x219224, new byte[] {0xB0, 0x00});
            Helpers.Instance.WriteToFile(ref Stream, 0x195C34, new byte[] {0xEB, 0x15});
            Helpers.Instance.WriteToFile(ref Stream, 0x69000F, new byte[] {0x94});
            Helpers.Instance.WriteToFile(ref Stream, 0x58BC2E, new byte[] {0x01});
        }
        public static FileStream HandleInput(string[] args)
        {
            FileStream Stream;
            string Filepath;

            if (args.Length == 0)
            {
                Console.WriteLine("S7Patcher: ERROR - Nothing passed as argument! Aborting ...");           
                return null;
            }

            Filepath = args[0];
            if (File.Exists(Filepath) == false)
            {
                Console.WriteLine("S7Patcher: ERROR - No valid file passed as argument! Aborting ...");
                return null;
            }

            if (Helpers.Instance.CreateBackup(Filepath) == false)
            {
                Console.WriteLine("S7Patcher: ERROR - Could not create backup of file! Aborting ...");
                return null;
            }

            Stream = Helpers.Instance.OpenFileStream(Filepath);
            if (Stream == null)
            {
                Console.WriteLine("S7Patcher: ERROR - Could not open FileStream! Aborting ...");
                return null;
            }

            return Stream;
        }
    }
}