using S7Patcher.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace S7Patcher.Source
{
    public sealed class Helpers
    {
        private Helpers() {}
        public static Helpers Instance {get;} = new();

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

        public void CloseFileStream(FileStream Stream)
        {
            Stream?.Close();
            Stream?.Dispose();
        }

        public string GetFileHash(FileStream Stream)
        {
            using MD5 MD = MD5.Create();
            return Convert.ToHexString(MD.ComputeHash(Stream)).ToLower();
        }

        public string RedirectLauncherFilePath(string ExecPath) => 
            Path.Combine(Path.GetDirectoryName(ExecPath), "Data", "Base", "_Dbg", "Bin", "Release", "Settlers7R.exe");

        public GameVariant? GetExecutableVariant(FileStream Stream)
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

            return null;
        }

        public void UpdateProfileXML(string Filepath)
        {
            List<string> Lines = ReadFileContent(Filepath) ?? [];

            int StartIndex = Lines.FindIndex(0, Element => Element.Contains("<TitleSystem>"));
            int EndIndex = Lines.FindIndex(0, Element => Element.Contains("</TitleSystem>"));
            int SynchIndex = Lines.FindIndex(0, Element => Element.Contains("<LastSynchTime>"));

            if (StartIndex == -1 || EndIndex == -1 || SynchIndex == -1)
            {
                return;
            }

            if ((EndIndex - StartIndex) != 4)
            {
                Lines.RemoveRange(StartIndex + 3, (EndIndex - StartIndex) - 4);
            }

            Lines[StartIndex + 2] = Resources.Branch;
            Lines[StartIndex + 3] = PopulateTitleSystem(Resources.Title);
            Lines[SynchIndex + 2] = Resources.Year;

            WriteFileContent(Filepath, Lines);
        }

        public void UpdateEntriesInOptionsFile(string Filepath)
        {
            List<string> Lines = ReadFileContent(Filepath);
            if (Lines == null)
            {
                return;
            }

            int SystemIndex = Lines.FindIndex(0, Element => Element.Contains("[System]"));
            if (SystemIndex != -1)
            {
                Lines.RemoveRange(SystemIndex - 1, Lines.Count - (SystemIndex - 1));
                Lines.Insert(SystemIndex - 1, Resources.Options);
            }
            else
            {
                Lines.Add(Resources.Options);
            }

            WriteFileContent(Filepath, Lines);
        }

        private string PopulateTitleSystem(string Input)
        {
            List<string> Collection = ["    <Titles>"];
            string Title = Input.Replace("%x2", "10");

            for (ushort TitleID = 0; TitleID < 4; TitleID++)
            {
                Collection.Add(Title.Replace("%x1", TitleID.ToString()));
            }

            Collection.Add(Input.Replace("%x1", "28").Replace("%x2", "0"));
            Collection.Add("    </Titles>");

            return string.Join("\r\n", Collection);
        }

        public byte GetAffinityMaskByte()
        {
            // Max 0xFF -> 255
            // ^ The above is wrong, since in x86 assembly a push is SIGN-EXTEND, meaning 7F (127) is the max value. 
            int Cores = Environment.ProcessorCount;
            Console.WriteLine("\n[INFO] Found " + Cores.ToString() + " processors!");
            Console.WriteLine("[INPUT] Input the physical cores the game should run on (7 at max) separated by ','.\n(Example: " +
                "Game should run on core 2 and 3 -> Input: 2,3)");

            do
            {
                int[] Input = ParseAffinityInput(Console.ReadLine());
                if (Input == null)
                {
                    continue;
                }

                byte Mask = 0x0;
                foreach (var Element in Input)
                {
                    Mask |= (byte)(1 << Element);
                }

                Console.WriteLine("\n[INFO] Writing Binary Mask " + Convert.ToString(Mask, 2).PadLeft(7, '0') + ".");
                return Mask;
            }
            while (true);
        }

        private int[] ParseAffinityInput(string Value)
        {
            if (string.IsNullOrWhiteSpace(Value))
            {
                Console.WriteLine("Input cannot be empty!");
                return null;
            }

            string Pattern = @"^[0-7](?:,[0-7])*$";
            if (Regex.IsMatch(Value, Pattern) == false)
            {
                Console.WriteLine("[ERROR] Erroneous input value. Please try again!");
                return null;
            }

            int[] Input = Value.Split(',').Length == 1 ? [int.Parse(Value)] : [.. Value.Split(',').Select(int.Parse)];
            if (Input.Length > 7)
            {
                Console.WriteLine("[ERROR] Erroneous input value. Please try again!");
                return null;
            }

            return Input;
        }

        private List<string> ReadFileContent(string Filepath)
        {
            try
            {
                return [.. File.ReadAllLines(Filepath, Encoding.UTF8)];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        private void WriteFileContent(string Filepath, List<string> Lines)
        {
            try
            {
                File.WriteAllLines(Filepath, Lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public int ConsoleReadWrapper()
        {
            int Value = Console.Read();
            if (Value == 13 || Value == 10)
            {
                return ConsoleReadWrapper();
            }

            return Value;
        }
    }
}
