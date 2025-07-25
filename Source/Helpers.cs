﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

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

        public string GetFileHash(FileStream Stream)
        {
            using MD5 MD = MD5.Create();
            return Convert.ToHexString(MD.ComputeHash(Stream)).ToLower();
        }

        public string RedirectLauncherFilePath(string ExecPath)
        {
            return Path.Combine(Path.GetDirectoryName(ExecPath), "Data", "Base", "_Dbg", "Bin", "Release", "Settlers7R.exe");
        }

        public GameVariant GetExecutableVariant(FileStream Stream)
        {
            byte[] Identifier = [0x8B, 0x01];
            byte[] Result = new byte[Identifier.Length];

            Dictionary<uint, GameVariant> Mapping = new()
            {
                {0x00D24C, GameVariant.ORIGINAL},
                {0xACC6C5, GameVariant.HE_STEAM},
                {0xACCFD5, GameVariant.HE_UBI}
            };

            foreach (var Element in Mapping)
            {
                if (Stream.Length < Element.Key)
                {
                    continue;
                }

                Stream.Position = Element.Key;
                Stream.ReadExactly(Result);

                if (Result.SequenceEqual(Identifier))
                {
                    return Element.Value;
                }
            }

            return GameVariant.NONE;
        }
    }
}
