using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            {GameVariant.HE_UBI,   0x2}
        };

        private readonly Tuple<GameVariant, UInt32>[] Affinities =
        [
            new(GameVariant.ORIGINAL, 0x62F0D2),
            new(GameVariant.HE_STEAM, 0x4589A1),
            new(GameVariant.HE_UBI,   0x458BD1)
        ];

        public bool PatchGameWrapper(Stream BinaryStream)
        {
            try
            {
                Parser = new(BinaryStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            if (Mapping.TryGetValue(GlobalID, out byte Value) == false)
            {
                Parser.Dispose();
                return false;
            }

            bool PatchError = PatchGame(Value);
            if (GlobalID == GameVariant.ORIGINAL)
            {
                UpdateConfigurationFile("Profiles.xml");
            }

            UpdateConfigurationFile("Options.ini");
            bool AffinityError = UpdateProcessAffinity(Value);

            Parser.Dispose();
            return PatchError && AffinityError;
        }

        private bool PatchGame(byte ID)
        {
            if (Parser.GetFileVersion() != Version)
            {
                Console.WriteLine("[ERROR] Binary Data Version Mismatch! Aborting ...");
                return false;
            }

            bool Error = WriteMapping(ID);
            bool DBGError = true;
            if (GlobalDebug)
            {
                DBGError = WriteMapping(ID, "DBG");
            }

            return Error && DBGError;
        }
        
        private bool WriteMapping(byte ID, string Block = "")
        {
            if (Parser.ParseBinaryFileContent(ID, out Dictionary<UInt32, byte[]> PatchMapping, Block) == false)
            {
                Console.WriteLine("[ERROR] Could not parse binary data! Aborting ...");
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
                    Console.WriteLine("\n[ERROR] " + Filepath + " not found!\n[INPUT] Please input the path to " +
                        "the " + Name + " file:\n(Input skip to skip file patching)");
                    Filepath = Console.ReadLine();

                    if (Filepath == "skip")
                    {
                        Console.WriteLine("\n[INFO] Skipping file patching ...");
                        return;
                    }
                    else if (File.Exists(Filepath))
                    {
                        break;
                    }
                }
                while (true);
            }

            Console.WriteLine("[INFO] Going to patch file: " + Filepath);
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
            Console.WriteLine("\n[INPUT] Update Process Affinity? (Enables higher framerate and smoother performance)\n(0 = Yes/1 = No):");
            int Input = Helpers.Instance.ConsoleReadWrapper();
            if (Input != '0')
            {
                Console.WriteLine("\n[INFO] Skipping Affinity ...");
                return true;
            }

            byte Mask = Helpers.Instance.GetAffinityMaskByte();
            Console.WriteLine("\n[INFO] Going to patch Affinity with value: 0x" + $"{Mask:X}");

            if (WriteMapping(ID, "AFF"))
            {
                Helpers.Instance.WriteToFile(GlobalStream, Affinities.FirstOrDefault(Element => Element.Item1 == GlobalID).Item2, [Mask]);
                return true;
            }

            return false;
        }
    }
}
