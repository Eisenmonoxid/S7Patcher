using System;
using System.IO;

namespace S7Patcher.Source
{
    public sealed class Helpers
    {
        private static readonly Helpers _instance = new();
        private Helpers() {}
        public static Helpers Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool CreateBackup(string Filepath)
        {
            string FinalPath = Path.Combine(Path.GetDirectoryName(Filepath), Path.GetFileNameWithoutExtension(Filepath) + "_BACKUP.exe");
            if (File.Exists(FinalPath) == false)
            {
                try
                {
                    File.Copy(Filepath, FinalPath, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }

            return true;
        }
        public void WriteToFile(FileStream Stream, long Position, byte[] Bytes)
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
        public FileStream OpenFileStream(string Path)
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
