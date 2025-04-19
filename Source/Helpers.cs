using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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

        public bool CreateBackup(string FilePath)
        {
            string FullPath = Path.Combine(Path.GetDirectoryName(FilePath), Path.GetFileNameWithoutExtension(FilePath) + "_BACKUP.exe");
            if (File.Exists(FullPath) == false)
            {
                try
                {
                    File.Copy(FilePath, FullPath, false);
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

        public bool CompareFileHash(FileStream Stream, string Hash)
        {
            using MD5 MD = MD5.Create();
            return Encoding.Default.GetBytes(Hash).SequenceEqual(MD.ComputeHash(Stream));
        }

        public string RedirectLauncherFilePath(string ExecPath)
        {
            return Path.Combine(Path.GetDirectoryName(ExecPath), "Data", "Base", "_Dbg", "Bin", "Release", "Settlers7R.exe");
        }
    }
}
