using S7Patcher.Properties;
using System;
using System.Collections.Generic;
using System.IO;

namespace S7Patcher.Source
{
    public sealed class Helpers
    {
        private static readonly Helpers _instance = new Helpers();
        private Helpers() {}
        public static Helpers Instance
        {
            get
            {
                return _instance;
            }
        }

        public void WriteToXMLFile(string Filepath)
        {
            string[] Lines = File.ReadAllLines(Filepath, System.Text.Encoding.UTF8);
            List<ushort> Indizes = new List<ushort>();
            bool Patch = true;
            ushort Cash = 0;
            for (ushort Index = 0; Index < Lines.Length; Index++)
            {
                if (Lines[Index].Contains("<Titles>") && Lines[Index + 1].Contains("</TitleSystem>"))
                {
                    Indizes.Add(Index);
                }
                if (Lines[Index].Contains("<Cash>"))
                {
                    Cash = Index;
                }
                if (Lines[Index].Contains("<CurrentTitleValue>"))
                {
                    Patch = false;
                    break;
                }
            }

            if (Patch == true)
            {
                foreach (ushort Index in Indizes)
                {
                    Lines[Index - 1] = Resources.Branch;
                    Lines[Index] = Resources.Title;
                    Lines[Cash] = Resources.Cash;
                }

                File.WriteAllLines(Filepath, Lines);
            }
        }
        public bool CreateBackup(string Filepath)
        {
            string FileName = Path.GetFileNameWithoutExtension(Filepath);
            string DirectoryPath = Path.GetDirectoryName(Filepath);
            string FinalPath = Path.Combine(DirectoryPath, FileName + "_BACKUP.exe");

            if (File.Exists(FinalPath) == false)
            {
                try
                {
                    File.Copy(Filepath, FinalPath, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("CreateBackup - ERROR: " + ex.ToString());
                    return false;
                }
            }

            return true;
        }
        public void WriteToFile(ref FileStream Stream, long Position, byte[] Bytes)
        {
            Stream.Position = Position;
            try
            {
                Stream.Write(Bytes, 0, Bytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WriteToFile - ERROR: " + ex.ToString());
            }
        }
        public FileStream OpenFileStream(string Path)
        {
            FileStream Stream;
            try
            {
                Stream = new FileStream(Path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine("OpenFileStream - ERROR: " + ex.ToString());
                return null;
            }

            return Stream;
        }
    }
}
