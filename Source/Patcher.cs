using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace S7Patcher.Source
{
    internal class Patcher(FileStream GlobalStream, GameVariant GlobalID, bool GlobalDebug)
    {
        private const byte Version = 0x1;
        private BinaryParser Parser;
        private readonly Dictionary<GameVariant, byte> Mapping = new()
        {
            {GameVariant.ORIGINAL, 0x0},
            {GameVariant.HE_STEAM, 0x1},
            {GameVariant.HE_UBI, 0x2}
        };

        public bool PatchGameWrapper()
        {
            Stream BinaryStream;
            try
            {
                BinaryStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("S7Patcher.Source.PatchData.bin");
                Parser = new(BinaryStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            if (Mapping.TryGetValue(GlobalID, out byte Value) == false)
            {
                return false;
            }

            bool Error = PatchGame(Value);
            if (GlobalID == GameVariant.ORIGINAL)
            {
                UpdateConfigurationFile("Profiles.xml");
            }

            UpdateConfigurationFile("Options.ini");
            Error = UpdateProcessAffinity(Value);

            Parser.Dispose();
            return Error;
        }

        private bool PatchGame(byte ID)
        {
            if (Parser.GetFileVersion() != Version)
            {
                Console.WriteLine("Error: Binary Data Version Mismatch! Aborting ...");
                return false;
            }

            bool Error = WriteMapping(ID);
            if (GlobalDebug)
            {
                Error = WriteMapping(ID, "DBG");
            }

            return Error;
        }
        
        private bool WriteMapping(byte ID, string Block = "")
        {
            if (Parser.ParseBinaryFileContent(ID, out Dictionary<UInt32, byte[]> PatchMapping, Block) == false)
            {
                Console.WriteLine("Error: Could not parse binary data! Aborting ...");
                return false;
            }

            foreach (var Entry in PatchMapping)
            {
                Helpers.Instance.WriteToFile(GlobalStream, Entry.Key, Entry.Value);
            }

            return true;
        }

        private void UpdateConfigurationFile(string Name)
        {
            string Folder = (GlobalID == GameVariant.ORIGINAL) ? "Settlers7" : "THE SETTLERS 7 - History Edition";
            string Filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Folder, Name);

            if (File.Exists(Filepath) == false)
            {
                do
                {
                    Console.WriteLine("\n" + Filepath + " not found!\nPlease input the path to " +
                        "the " + Name + " file:\n(Input skip to skip file patching)");
                    Filepath = Console.ReadLine();

                    if (Filepath == "skip")
                    {
                        Console.WriteLine("Skipping file patching ...");
                        return;
                    }
                    else if (File.Exists(Filepath))
                    {
                        break;
                    }
                }
                while (true);
            }

            Console.WriteLine("Going to patch file: " + Filepath);
            if (Name == "Profiles.xml")
            {
                Helpers.Instance.UpdateProfileXML(Filepath);
            }
            else if (Name == "Options.ini")
            {
                Helpers.Instance.UpdateEntriesInOptionsFile(Filepath);
            }
        }

        private bool UpdateProcessAffinity(byte ID)
        {
            Console.WriteLine("\nUpdate Process Affinity? (Enables higher framerate and smoother performance)\n(0 = Yes/1 = No):");
            int Input = Console.Read();
            if (Input != '0')
            {
                Console.WriteLine("Skipping Affinity ...");
                return false;
            }

            byte Mask = Helpers.Instance.GetAffinityMaskByte();
            Console.WriteLine("Going to patch Affinity with value: 0x" + $"{Mask:X}");

            if (WriteMapping(ID, "AFF"))
            {
                long Position;
                switch (GlobalID)
                {
                    case GameVariant.ORIGINAL:
                        Position = 0x62F030;
                        break;
                    case GameVariant.HE_STEAM:
                        Position = 0x45899F;
                        break;
                    case GameVariant.HE_UBI:
                        Position = 0x458BD1;
                        break;
                    default:
                        return false;
                }

                Helpers.Instance.WriteToFile(GlobalStream, Position, [Mask]);
                return true;
            }

            return false;
        }
    }
}
