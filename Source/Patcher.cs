using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace S7Patcher.Source
{
    internal class Patcher
    {
        private const byte Version = 0x1;
        private readonly BinaryParser Parser;
        private readonly FileStream GlobalStream;
        private readonly GameVariant GlobalID;
        private readonly bool GlobalDebug;
        private readonly byte Identifier;

        private readonly Dictionary<GameVariant, byte> Mapping = new()
        {
            {GameVariant.ORIGINAL, 0x0},
            {GameVariant.HE_STEAM, 0x1},
            {GameVariant.HE_UBI,   0x2}
        };

        private readonly Dictionary<GameVariant, UInt32> Affinities = new()
        {
            {GameVariant.ORIGINAL, 0x62F0D2},
            {GameVariant.HE_STEAM, 0x4589A2},
            {GameVariant.HE_UBI,   0x458BD2} 
        };

        public Patcher(FileStream Stream, Stream BinaryStream, GameVariant ID, bool Debug)
        {
            GlobalStream = Stream;
            GlobalID = ID;
            GlobalDebug = Debug;

            try
            {
                Parser = new(BinaryStream);
            }
            catch (Exception ex)
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.ERROR, ex.Message);
                throw;
            }

            if (!Mapping.TryGetValue(GlobalID, out byte Result))
            {
                Parser.Dispose();
                Helpers.Instance.WriteWrapper(ConsoleColorType.ERROR, "Invalid Game Variant! Aborting ...");
                throw new Exception();
            }

            Identifier = Result;
        }

        public bool PatchGameWrapper()
        {
            bool Error = PatchGame(Identifier);
            if (GlobalID == GameVariant.ORIGINAL)
            {
                UpdateConfigurationFile("Profiles.xml");
            }

            UpdateConfigurationFile("Options.ini");
            Error &= UpdateProcessAffinity(Identifier);

            Parser.Dispose();
            return Error;
        }

        private bool PatchGame(byte ID)
        {
            if (Parser.GetFileVersion() != Version)
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.ERROR, "Binary Data Version Mismatch! Aborting ...");
                return false;
            }

            bool Error = WriteMapping(ID);
            if (GlobalDebug)
            {
                Error &= WriteMapping(ID, "DBG");
            }

            return Error;
        }
        
        private bool WriteMapping(byte ID, string Block = "")
        {
            if (!Parser.ParseBinaryFileContent(ID, out Dictionary<UInt32, byte[]> Mapping, Block))
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.ERROR, "Could not parse binary data! Aborting ...");
                return false;
            }

            foreach (var Entry in Mapping)
            {
                Helpers.Instance.WriteToFile(GlobalStream, Entry.Key, Entry.Value);
            }

            return true;
        }

        private void UpdateConfigurationFile(string Name)
        {
            string Folder = (GlobalID == GameVariant.ORIGINAL) ? "Settlers7" : "THE SETTLERS 7 - History Edition";
            string Filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Folder, Name);

            if (!File.Exists(Filepath))
            {
                do
                {
                    Helpers.Instance.WriteWrapper(ConsoleColorType.ERROR, Filepath + " not found!");
                    Helpers.Instance.WriteWrapper(ConsoleColorType.INPUT, "Please input the path to the " + Name + " file:" +
                        "\n(Input skip to skip file patching)");
                    Filepath = Console.ReadLine();

                    if (Filepath == "skip")
                    {
                        Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "Skipping file patching ...");
                        return;
                    }
                    else if (File.Exists(Filepath))
                    {
                        break;
                    }
                }
                while (true);
            }

            Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "Going to patch file: " + Filepath);
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
            Helpers.Instance.WriteWrapper(ConsoleColorType.INPUT, "Update Process Affinity? (Enables higher framerate and smoother performance)\n(0 = Yes/1 = No):");

            string Input = Console.ReadLine();
            if (Input != "0")
            {
                Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "Skipping Affinity ...");
                return true;
            }

            byte Mask = Helpers.Instance.GetAffinityMaskByte();
            Helpers.Instance.WriteWrapper(ConsoleColorType.INFO, "Going to patch Affinity with value: 0x" + $"{Mask:X}");

            if (WriteMapping(ID, "AFF"))
            {
                Helpers.Instance.WriteToFile(GlobalStream, Affinities.FirstOrDefault(Element => Element.Key == GlobalID).Value, [Mask]);
                return true;
            }

            return false;
        }
    }
}
